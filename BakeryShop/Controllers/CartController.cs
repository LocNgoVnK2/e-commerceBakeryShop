using AutoMapper;
using BakeryShop.Models;

using BakeryShop.Utils;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;
using System.Transactions;

namespace BakeryShop.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductsService _productsService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ICheckOutService _checkOutService;
        private readonly Infrastructure.Service.IEmailService emailService;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;
        private readonly IStoreService _storeService;
        private readonly IMapper _mapper;

        public CartController(IProductsService productsService,
                              IOrderDetailService orderDetailService,
                              IOrderService orderService,
                              ICheckOutService checkOutService,
                              ICustomerService customerService,
                              Infrastructure.Service.IEmailService emailService,
                              IVnPayService vnPayService,
                              IConfiguration configuration,
                              IStoreService storeService,
                              IMapper mapper
                              )
        {
            this._productsService = productsService;
            _orderDetailService = orderDetailService;
            _orderService = orderService;
            _customerService = customerService;
            _checkOutService = checkOutService;
            this.emailService = emailService;
            _storeService = storeService;
            _vnPayService = vnPayService;
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<ActionResult> Index(int id, int quantity)
         {
            if (id != 0)
            {
                var oldItemData = HttpContext.Session.GetString("cart");
                if (oldItemData != null)
                { // thêm vào card đã có item

                    List<CartItemVewModel> oldItemList = new List<CartItemVewModel>();

                    oldItemList = JsonConvert.DeserializeObject<List<CartItemVewModel>>(oldItemData);

                    Product product = await _productsService.GetProduct(id);
                    if (oldItemList.FirstOrDefault(item => item.ProductId == product.ProductID) != null)
                    {
                        oldItemList.FirstOrDefault(item => item.ProductId == product.ProductID).Quantity += quantity;
                    }
                    else
                    {
                        CartItemVewModel cartItemVewModel = new CartItemVewModel()
                        {
                            Price = (Double)product.Price,
                            Quantity = quantity,
                            ProductName = product.ProductName,
                            ProductImage = product.Image,
                            ProductId = product.ProductID,
                            AvailableQuantity = (int)product.Quantity
                        };
                        oldItemList.Add(cartItemVewModel); // Thêm sản phẩm mới vào danh sách
                    }
                    string serializedItemList = JsonConvert.SerializeObject(oldItemList);
                    HttpContext.Session.SetString("cart", serializedItemList);

                }
                else
                { // thêm mới cart còn trẳng
                    Product product = await _productsService.GetProduct(id);
                    List<CartItemVewModel> cartItemVewModels = new List<CartItemVewModel>();
                    CartItemVewModel cartItemVewModel = new CartItemVewModel()
                    {
                        Price = (Double)product.Price,
                        Quantity = quantity,
                        ProductName = product.ProductName,
                        ProductImage = product.Image,
                        ProductId = product.ProductID,
                        AvailableQuantity = (int)product.Quantity
                    };
                    cartItemVewModels.Add(cartItemVewModel);
                    var item = cartItemVewModels;

                    // Lưu danh sách cartItemVewModels vào session
                    string serializedItemList = JsonConvert.SerializeObject(item);
                    HttpContext.Session.SetString("cart", serializedItemList);
                }

            }
            var cartItemData = HttpContext.Session.GetString("cart");
            List<CartItemVewModel> cartItemList = new List<CartItemVewModel>();

            if (!string.IsNullOrEmpty(cartItemData))
            {
                cartItemList = JsonConvert.DeserializeObject<List<CartItemVewModel>>(cartItemData);
            }

            return View(cartItemList);
        }

        public async Task<ActionResult> CheckOutBill()
        {
                var cartItemData = HttpContext.Session.GetString("cart");
                List<CartItemVewModel> cartItemList = new List<CartItemVewModel>();
                List<string> outOfStockProducts = new List<string>(); // Danh sách sản phẩm hết số lượng
                if (!string.IsNullOrEmpty(cartItemData))
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            Order order = new Order()
                            {
                                OrderDate = DateTime.Now,
                                IsDone = false
                            };

                            CheckOutViewModel checkOut = new CheckOutViewModel();
                            List<RollBackViewModel> list = new List<RollBackViewModel>();
                            await _orderService.InsertOrder(order);
                            Double totalPrice = 0;

                    
                                cartItemList = JsonConvert.DeserializeObject<List<CartItemVewModel>>(cartItemData);

                                foreach (CartItemVewModel cartItemVew in cartItemList)
                                {
                                    // Kiểm tra số lượng sản phẩm
                                    Product product = await _productsService.GetProduct(cartItemVew.ProductId);
                                    if (product.Quantity < cartItemVew.Quantity)
                                    {

                                        outOfStockProducts.Add(product.ProductName + "Chỉ còn " + product.Quantity + " Sản phẩm");
                                        continue;
                                    }

                                    // Cập nhật số lượng cho sản phẩm
                                    product.Quantity -= cartItemVew.Quantity;

                                    RollBackViewModel rollBack = new RollBackViewModel();
                                    rollBack.Quantity = cartItemVew.Quantity;
                                    rollBack.Id = cartItemVew.ProductId;
                                    list.Add(rollBack);

                                    await _productsService.UpdateProduct(product);

                                    totalPrice += cartItemVew.TotalPrice;
                                    OrderDetail orderDetail = new OrderDetail()
                                    {
                                        ProductID = cartItemVew.ProductId,
                                        OrderID = order.OrderID,
                                        Quantity = cartItemVew.Quantity,
                                        Subtotal = cartItemVew.TotalPrice
                                        // discount tính sau 
                                    };
                                    await _orderDetailService.InsertOrderDetail(orderDetail);
                                }

                                order.TotalAmount = totalPrice;
                                await _orderService.UpdateOrder(order);
                        

                            scope.Complete();

                            if (outOfStockProducts.Any())
                            {
                                // Thêm thông báo vào TempData nếu có sản phẩm hết số lượng
                                TempData["OutOfStockProducts"] = string.Join(", ", outOfStockProducts);
                                return RedirectToAction("Index");
                            }

                            checkOut.IdOrder = order.OrderID;
                            checkOut.TotalPrice = order.TotalAmount;
                            checkOut.RollBacks = list;

                            string serializedItemList = JsonConvert.SerializeObject(checkOut);
                            HttpContext.Session.SetString("RollBackList", serializedItemList);

                            return View(checkOut);
                        }
                        catch (Exception ex)
                        {
                            scope.Dispose();
                            return NotFound();
                        }
                }
            }
            else
            {
                // use rollback
                var oldItemData = HttpContext.Session.GetString("RollBackList");
                CheckOutViewModel checkOut = JsonConvert.DeserializeObject<CheckOutViewModel>(oldItemData);
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        await ProcessRollback(checkOut);

                        scope.Complete();

                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();

                    }
                }
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<IActionResult> RollBackOrder()
        {
            try
            {
                var oldItemData = HttpContext.Session.GetString("RollBackList");
                CheckOutViewModel checkOut = JsonConvert.DeserializeObject<CheckOutViewModel>(oldItemData);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        await ProcessRollback(checkOut);

                        scope.Complete();
                        return Json("Pass");
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return Json(new { success = false });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        private async Task ProcessRollback(CheckOutViewModel checkOut)
        {
  
            Order order = await _orderService.GetOrder((int)checkOut.IdOrder);
            IQueryable<OrderDetail> orderDetail = await _orderDetailService.GetOrderDetailsByOrderId((int)order.OrderID);
            IQueryable<Product> products = await _productsService.GetProducts();

            foreach (var item in checkOut.RollBacks)
            {
                var productToUpdate = products.FirstOrDefault(p => p.ProductID == item.Id);

                if (productToUpdate != null)
                {
                    productToUpdate.Quantity += item.Quantity;
                    await _productsService.UpdateProduct(productToUpdate);
                }
            }

            foreach (var item in orderDetail)
            {
                await _orderDetailService.DeleteOrderDetail(item);
            }

            await _orderService.DeleteOrder(order);
            
        }


        public async Task<ActionResult> CompleteCheckOut(CheckOutViewModel checkOutView)
        {
            int store = await FindNearStoreFromOrder(checkOutView.Address);
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                try
                {
                    Order order = await _orderService.GetOrder((int)checkOutView.IdOrder);
                    Customer customer = new Customer()
                    {
                        Address = checkOutView.Address,
                        Email = checkOutView.Email,
                        FirstName = checkOutView.FirstName,
                        LastName = checkOutView.LastName,
                        PhoneNumber = checkOutView.PhoneNumber,

                    };
                    await _customerService.InsertCustomer(customer);
                    CheckOut checkOut = new CheckOut();
                    checkOut.IdOrder = checkOutView.IdOrder;
                    checkOut.CustomerId = customer.CustomerId;
                    checkOut.IsReceived = false;
                    checkOut.Note = checkOutView.Note;
                    await _checkOutService.InsertCheckOut(checkOut);

                    //set status payment
                    order.PaidStatus = false;
                    order.IdStore = store;
                    await _orderService.UpdateOrder(order);

                    scope.Complete();
                    HttpContext.Session.Remove("cart");

                    return RedirectToAction("ValidationReviewOrder", "Cart");
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return NotFound();

                }
            }

        }
        public async Task<ActionResult> ReviewOrder(string validationCode, string phoneNumber)
        {

            if ((validationCode == null && phoneNumber == null) && (TempData["ValidationCode"] != null && TempData["PhoneNumber"] != null))
            {
                validationCode = TempData["ValidationCode"] as string;
                phoneNumber = TempData["PhoneNumber"] as string;
            }
            if (validationCode == null)
            {
                return View();
            }
            else
            {
                List<CheckOutViewModel> checkOutViewModels = new List<CheckOutViewModel>();
                Customer customer = await _customerService.GetCustomerByPhoneNumber(phoneNumber);
                // get last record of this customer to validation

                if (customer != null && customer.ValidationCode == validationCode)
                {
                    List<Customer> customers = await _customerService.GetCustomersByPhoneNumber(phoneNumber);
                    foreach (Customer cus in customers)
                    {
                        List<CheckOut> checkOuts = await _checkOutService.GetListCheckOutByCustomerId((int)cus.CustomerId);
                        foreach (CheckOut checkOut in checkOuts)
                        {
                            Order order = await _orderService.GetOrder((int)checkOut.IdOrder);
                            CheckOutViewModel checkOutView = new CheckOutViewModel()
                            {

                                IdOrder = checkOut.IdOrder,
                                IsReceived = checkOut.IsReceived,
                                IsAccept = order.IsDone,
                                Note = checkOut.Note,
                                TotalPrice = order.TotalAmount,
                                OrderDate = order.OrderDate,
                                PhoneNumber = phoneNumber
                            };
                            checkOutViewModels.Add(checkOutView);
                        }
                    }
                    TempData.Remove("ValidationCode");
                    TempData.Remove("PhoneNumber");
                    return View(checkOutViewModels);
                }
            }

            return RedirectToAction("ValidationReviewOrder");
        }
        public async Task<ActionResult> ValidationReviewOrder()
        {

            return View();

        }
        //ValidateUser
        public async Task<ActionResult> ValidateUser(string phoneNumber)
        {
            if (phoneNumber == null)
            {
                return RedirectToAction("ValidationReviewOrder");
            }
            try
            {
                Customer customer = await _customerService.GetCustomerByPhoneNumber(phoneNumber);
                if (customer != null)
                {

                    customer.ValidationCode = GenerateRandomString(6);
                    await _customerService.UpdateCustomer(customer);
                    //sendmail
                    string subject = "Xin chào: " + customer.FirstName + " " + customer.LastName + " | Email đăng nhập Tho Bakery";
                    string content = "Đây là email gửi tự động bởi hệ thống xác minh, Mã xác nhận của bận :" + customer.ValidationCode;
                    var message = new EmailMessage(customer.Email, subject, content);
                    emailService.SendEmail(message);

                    return RedirectToAction("ReviewOrder", new { phoneNumber = phoneNumber });

                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ValidationReviewOrder");
            }
            return RedirectToAction("ValidationReviewOrder");

        }
        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }





        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int newQuantity)
        {
            var cartItemData = HttpContext.Session.GetString("cart");
            if (!string.IsNullOrEmpty(cartItemData))
            {
                List<CartItemVewModel> cartItemList = JsonConvert.DeserializeObject<List<CartItemVewModel>>(cartItemData);
                var cartItemToUpdate = cartItemList.FirstOrDefault(item => item.ProductId == productId);
                if (cartItemToUpdate != null)
                {

                    cartItemToUpdate.Quantity = newQuantity;
                    string serializedItemList = JsonConvert.SerializeObject(cartItemList);
                    HttpContext.Session.SetString("cart", serializedItemList);
                    double newTotalPrice = cartItemList.Sum(item => item.TotalPrice);
                    var responseData = new
                    {
                        success = true,
                        totalPrice = newTotalPrice,
                        totalQuantity = cartItemList.Count
                    };

                    return Json(responseData);
                }
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public IActionResult RemoveProduct(int productId)
        {
            var cartItemData = HttpContext.Session.GetString("cart");
            if (!string.IsNullOrEmpty(cartItemData))
            {
                List<CartItemVewModel> cartItemList = JsonConvert.DeserializeObject<List<CartItemVewModel>>(cartItemData);
                var cartItemToRemove = cartItemList.FirstOrDefault(item => item.ProductId == productId);
                if (cartItemToRemove != null)
                {
                    cartItemList.Remove(cartItemToRemove); // Xóa sản phẩm khỏi danh sách giỏ hàng
                    string serializedItemList = JsonConvert.SerializeObject(cartItemList);
                    HttpContext.Session.SetString("cart", serializedItemList);
                    double newTotalPrice = cartItemList.Sum(item => item.TotalPrice);
                    var responseData = new
                    {
                        success = true,
                        totalPrice = newTotalPrice,
                        totalQuantity = cartItemList.Count
                    };

                    return Json(responseData);
                }
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> CancelOrder(int orderId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                string validationCode = "";
                string phoneNumber = "";
                try
                {
                    Order order = await _orderService.GetOrder(orderId);
                    CheckOut checkOut = await _checkOutService.GetCheckOut(orderId);
                    Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);
                    Customer flagCustomer = await _customerService.GetCustomerByPhoneNumber(customer.PhoneNumber); // lấy last mới đúng là status của customer đang login
                    validationCode = flagCustomer.ValidationCode;
                    phoneNumber = flagCustomer.PhoneNumber;
                    IQueryable<OrderDetail> orderDetails = await _orderDetailService.GetOrderDetailsByOrderId(orderId);

                    await _checkOutService.DeleteCheckOut(checkOut);
                    foreach (OrderDetail detail in orderDetails)
                    {
                        await _orderDetailService.DeleteOrderDetail(detail);
                    }
                    await _orderService.DeleteOrder(order);
                    TempData["ValidationCode"] = validationCode;
                    TempData["PhoneNumber"] = phoneNumber;

                    scope.Complete();
                    return RedirectToAction("ReviewOrder");
                    //return RedirectToAction("ReviewOrder", new { validationCode = validationCode , phoneNumber =phoneNumber});
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return NotFound();
                }
            }



        }

        public async Task<IActionResult> CreatePaymentUrl(CheckOutViewModel checkOutView)
        {
            HttpContext.Session.Remove("cart");
            Order order = await _orderService.GetOrder((int)checkOutView.IdOrder);

            Infrastructure.Entities.PaymentInformationModel model = new Infrastructure.Entities.PaymentInformationModel();
            model.Name = checkOutView.FirstName + " " + checkOutView.LastName;
            model.Amount = (double)order.TotalAmount;
            //model.Amount = 200000;
            model.OrderDescription = "thanh toán cho hóa đơn: '" + checkOutView.IdOrder + "' với giá :";
            TempData["checkOutViewModel"] = JsonConvert.SerializeObject(checkOutView);
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
        
            return Redirect(url);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var responseModel = _vnPayService.PaymentExecute(Request.Query);

            var response = _mapper.Map<Models.PaymentResponseModel>(responseModel); 

            if (response.TransactionId != "0" && response.PaymentId != "0")
            {


                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var checkOutView = JsonConvert.DeserializeObject<CheckOutViewModel>(TempData["checkOutViewModel"] as string);

                        Customer customer = new Customer()
                        {
                            Address = checkOutView.Address,
                            Email = checkOutView.Email,
                            FirstName = checkOutView.FirstName,
                            LastName = checkOutView.LastName,
                            PhoneNumber = checkOutView.PhoneNumber,
                        };
                        await _customerService.InsertCustomer(customer);
                        CheckOut checkOut = new CheckOut();
                        checkOut.IdOrder = checkOutView.IdOrder;
                        checkOut.CustomerId = customer.CustomerId;
                        checkOut.IsReceived = false;
                        checkOut.Note = checkOutView.Note;
                        await _checkOutService.InsertCheckOut(checkOut);

                        //set status payment
                        Order order = await _orderService.GetOrder((int)checkOutView.IdOrder);
                        order.PaidStatus = true;
                        await _orderService.UpdateOrder(order);

                        scope.Complete();
                        HttpContext.Session.Remove("cart");
                        HttpContext.Session.Remove("RollBackList");
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                    }
                }

                return View(response);
            }
            else
            {
                       // use rollback
                    var oldItemData = HttpContext.Session.GetString("RollBackList");
                    CheckOutViewModel checkOut = JsonConvert.DeserializeObject<CheckOutViewModel>(oldItemData);
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            await ProcessRollback(checkOut);

                            scope.Complete();
                          
                        }
                        catch (Exception ex)
                        {
                            scope.Dispose();
                    
                        }
                    }
              
                    return RedirectToAction("Index", "Home");
                
               
            }
        }


        public async Task<string> DistanceCalculate(string origins, string destinations)
        {

            string apiUrl = "https://api.distancematrix.ai/maps/api/";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiKey = _configuration["DistancematrixSecretKey"];

                    string request = apiUrl + "distancematrix/json?origins=" + origins + "&destinations=" + destinations + "&key=" + apiKey;
                    HttpResponseMessage response = await client.GetAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        JToken jsonToken = JToken.Parse(responseData);

                        // Lấy giá trị số kilomet từ JSON
                        string distanceText = jsonToken["rows"][0]["elements"][0]["distance"]["text"].ToString();
                        // trả về định dạnh "xx km"

                        return distanceText;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }

        }
        public async Task<int> FindNearStoreFromOrder(string address)
        {
            var listStore = await _storeService.GetStores();
            int nearStoreId = -1;
            double minDistance = double.MaxValue;

            foreach (var store in listStore)
            {
                string km = await DistanceCalculate(store.Address, address);
                double distance = Utils.Utils.ExtractDistance(km);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearStoreId = store.IdStore; // Giả sử cửa hàng có một thuộc tính Id để lấy.
                }
            }

            return nearStoreId;
        }


    }
}
