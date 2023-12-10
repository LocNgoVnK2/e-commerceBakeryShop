using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Transactions;
using X.PagedList;

using System.IO;
using System.Text;
using System.Data;

using MailKit.Search;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using System.Globalization;

namespace BakeryShop.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IProductsService _productsService;
        private readonly IAccountsService _accountsService;
        private readonly IEmployeeService _employeeService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ICheckOutService _checkOutService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IRateService _rateService;
        private readonly IStoreService _storeService;

        public DashBoardController(IMapper mapper,
                                    ICategoryService categoryService,
                                    IProductsService productsService,
                                    IEmployeeService employeeService,
                                    IAccountsService accountsService,
                                    ICheckOutService checkOutService,
                                    IOrderService orderService,
                                    ICustomerService customerService,
                                    IOrderDetailService orderDetailService,
                                    IRateService rateService,
                                    IStoreService storeService

                                    )
        {
            _mapper = mapper;
            _categoryService = categoryService;
            _productsService = productsService;
            _employeeService = employeeService;
            _accountsService = accountsService;
            _orderService = orderService;
            _customerService = customerService;
            _checkOutService = checkOutService;
            _orderDetailService = orderDetailService;
            _rateService = rateService;
            _storeService = storeService;
        }

        public async Task<IActionResult> Index()
        {

            List<CheckOutBillViewModel> checkOutViewModels = new List<CheckOutBillViewModel>();

            string userJson = HttpContext.Session.GetString("LoggedInUser");
            AccountsViewModel loggedInUser = JsonConvert.DeserializeObject<AccountsViewModel>(userJson);


            var orders = await _orderService.GetOrders();
            if(loggedInUser.Role == "1")
            {
                orders = orders.Where(e => e.IsDone == false).Select(e => e);
            }
            else
            {
                orders = orders.Where(e => e.IsDone == false && e.IdStore == loggedInUser.IdStore).Select(e => e);
            }
   


            foreach (Order order in orders)
            {


                CheckOut checkOut = await _checkOutService.GetCheckOut((int)order.OrderID);
                if (checkOut != null)
                {
                    Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);
                    CheckOutBillViewModel checkOutView = new CheckOutBillViewModel()
                    {

                        IdOrder = checkOut.IdOrder,
                        IsReceived = checkOut.IsReceived,
                        TotalPrice = order.TotalAmount,
                        OrderDate = order.OrderDate,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        FirstName = customer.FirstName + " " + customer.LastName,
                        PaymentStatus = (bool)order.PaidStatus
                    };
                    checkOutViewModels.Add(checkOutView);

                }

            }
            return View(checkOutViewModels);

        }
        //CheckOutCompleteBill
        public async Task<IActionResult> CheckOutCompleteBill(string inforOrder) // load ở view chop nhan vien hoan thanh dpon hang

        {

            List<CheckOutBillViewModel> checkOutViewModels = new List<CheckOutBillViewModel>();

            var orders = await _orderService.GetOrders();
            orders = orders.Where(e => e.IsDone == true).Select(e => e);

            foreach (Order order in orders)
            {
                CheckOut checkOut = await _checkOutService.GetCheckOut((int)order.OrderID);

                if (checkOut != null && checkOut.IsReceived==false)
                {
                    Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);
                    CheckOutBillViewModel checkOutView = new CheckOutBillViewModel()
                    {

                        IdOrder = checkOut.IdOrder,
                        IsReceived = checkOut.IsReceived,
                        TotalPrice = order.TotalAmount,
                        OrderDate = order.OrderDate,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        FirstName = customer.FirstName + " " + customer.LastName,
                        PaymentStatus = (bool)order.PaidStatus,
                        DeliveryStatus = order.DeliveryId == null ? false : true

                    };
                    checkOutViewModels.Add(checkOutView);

                }

            }

            if (!string.IsNullOrEmpty(inforOrder))
            {
                checkOutViewModels = checkOutViewModels.Where(checkOut =>
                    checkOut.IdOrder.ToString().Contains(inforOrder) ||
                    checkOut.TotalPrice.ToString().Contains(inforOrder) ||
                    checkOut.OrderDate.ToString().Contains(inforOrder) ||
                    checkOut.PhoneNumber.Contains(inforOrder) ||
                    checkOut.Address.Contains(inforOrder) ||
                    checkOut.FirstName.Contains(inforOrder)
                ).ToList();
            }
            return View(checkOutViewModels);

        }
        public async Task<IActionResult> CheckOutCompleteForAdmin(string searchString )// lay hoa don cho tk admin
        {
            List<CheckOutBillViewModel> checkOutViewModels = new List<CheckOutBillViewModel>();

            var orders = await _orderService.GetOrders();
            if (!string.IsNullOrEmpty(searchString))
            {
               
                orders = orders
                    .Where(order =>
                        order.OrderID.ToString().Contains(searchString) ||
                        order.OrderDate.ToString().Contains(searchString) 
 

                    );
            }
            else
            {
                orders = orders.Where(e => e.IsDone == true).Select(e => e);
            }
          
            foreach (Order order in orders)
            {
                CheckOut checkOut = await _checkOutService.GetCheckOut((int)order.OrderID);

                if (checkOut != null && checkOut.IsReceived == true)
                {
                    Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);
                    CheckOutBillViewModel checkOutView = new CheckOutBillViewModel()
                    {

                        IdOrder = checkOut.IdOrder,
                        IsReceived = checkOut.IsReceived,
                        TotalPrice = order.TotalAmount,
                        OrderDate = order.OrderDate,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        FirstName = customer.FirstName + " " + customer.LastName,
                        PaymentStatus = (bool)order.PaidStatus
                    };
                    checkOutViewModels.Add(checkOutView);

                }

            }
            return View(checkOutViewModels);

        }
        public async Task<IActionResult> Product(int? page, string searchString)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = (page ?? 1);


                IQueryable<Product> listProduct = await _productsService.GetProducts();
                if (!String.IsNullOrEmpty(searchString))
                {
                    listProduct = listProduct.Where(e => e.IsUsed == true && e.ProductName.Contains(searchString)).Select(e => e);
                }
                else
                {
                    listProduct = listProduct.Where(e => e.IsUsed == true).Select(e => e);
                }

                List<ProductViewModel> products = _mapper.Map<List<ProductViewModel>>(listProduct.ToList());
                IQueryable<Category> categories = await _categoryService.GetCategories();
                foreach (ProductViewModel product in products)
                {

                    product.Category = categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
                }
                IPagedList<ProductViewModel> pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);


                return View("Product", pagedProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        public async Task<IActionResult> AddProduct()
        {
            ProductViewModel model = new ProductViewModel();

            IQueryable<Category> categories = await _categoryService.GetCategories();
            IEnumerable<CategoryViewModel> categoriesModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            model.Categories = categoriesModel;

            return View("AddProduct", model);
        }
        public async Task<IActionResult> AddUserAccount()
        {
            IQueryable<Store> stores = await _storeService.GetStores();
            IEnumerable<StoreViewModel> storesModel = _mapper.Map<IEnumerable<StoreViewModel>>(stores);

            AccountManagementViewModel accountManagementViewModel = new AccountManagementViewModel();
            accountManagementViewModel.Stores = storesModel;

            return View("AddUserAccount", accountManagementViewModel);
        }
        public async Task<IActionResult> EditProduct(int id)
        {
            Product product = await _productsService.GetProduct(id);
            ProductViewModel productView = _mapper.Map<ProductViewModel>(product);
            IQueryable<Category> categories = await _categoryService.GetCategories();
            IEnumerable<CategoryViewModel> categoriesModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            productView.Categories = categoriesModel;

            return View("EditProduct", productView);
        }
        public async Task<IActionResult> EditCategory(int id)
        {
            Category category = await _categoryService.GetCategory(id);
            CategoryViewModel categoryView = _mapper.Map<CategoryViewModel>(category);
            return View("EditCategory", categoryView);
        }
        public async Task<IActionResult> Category()
        {
            try
            {
                var categories = await _categoryService.GetCategories();
                var categoryViews = _mapper.Map<IEnumerable<CategoryViewModel>>(categories.AsEnumerable());

                var categoryPageViewModel = new CategoryPageViewModel
                {
                    Categories = categoryViews,
                    NewCategory = new CategoryViewModel()
                };
                return View("Category", categoryPageViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> AccountManagement(int? page, string searchString)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = (page ?? 1);

                IQueryable<Accounts> listAccounts = await _accountsService.GetAccounts();
                IQueryable<Employee> listEmployees = await _employeeService.GetEmployees();
                if (!String.IsNullOrEmpty(searchString))
                {
                    listAccounts = listAccounts.Where(e => e.Username.Contains(searchString)).Select(e => e);

                }

                var query =
                       from Account in listAccounts
                       join Employee in listEmployees on Account.EmployeeID equals Employee.EmployeeID

                       select new
                       {
                           Employee,
                           Account
                       };
                List<AccountManagementViewModel> viewModels = new List<AccountManagementViewModel>();
                foreach (var v in query)
                {
                    AccountManagementViewModel viewModel = new AccountManagementViewModel()
                    {
                        AccountID = v.Account.AccountID,
                        Username = v.Account.Username,
                        Password = v.Account.Password,
                        Email = v.Account.Email,
                        Role = v.Account.Role,
                        EmployeeID = v.Employee.EmployeeID,
                        FirstName = v.Employee.FirstName,
                        LastName = v.Employee.LastName,
                        Position = v.Employee.Position,
                        PhoneNumber = v.Employee.PhoneNumber,
                        IsActivate = v.Account.IsActivate
                    };
                    viewModels.Add(viewModel);
                }
                IPagedList<AccountManagementViewModel> pagedAccountManagementModels = await viewModels.ToPagedListAsync(pageNumber, pageSize);
                return View("AccountManagement", pagedAccountManagementModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //EditAccount
        public async Task<IActionResult> EditAccount(int id)
        {
            Accounts account = await _accountsService.GetAccount(id);
            Employee employee = await _employeeService.GetEmployee((int)account.EmployeeID);
            IEnumerable<Store> stores = await _storeService.GetStores();
            IEnumerable<StoreViewModel> storesModel = _mapper.Map<IEnumerable<StoreViewModel>>(stores);

            AccountManagementViewModel model = new AccountManagementViewModel();
            if (employee != null)
            {
                model.Email = account.Email;
                model.Username = account.Username;
                model.AccountID = account.AccountID;
                model.EmployeeID = employee.EmployeeID;
                model.PhoneNumber = employee.PhoneNumber;
                model.FirstName = employee.FirstName;
                model.LastName = employee.LastName;
                model.Position = employee.Position;
                model.IdStore = account.IdStore;
                model.Stores = storesModel;

            }
            return View("EditAccount", model);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            CheckOutBillViewModel checkOutView = await QueryOrderDetail(orderId);


            return Json(checkOutView);

        }
        public async Task<CheckOutBillViewModel> QueryOrderDetail(int orderId)
        {

            var order = await _orderService.GetOrder(orderId);
            var orderDetails = await _orderDetailService.GetOrderDetailsByOrderId(orderId);
            List<OrderDetailViewModel> orderDetailViewModels = new List<OrderDetailViewModel>();
            foreach (var orderDetail in orderDetails)
            {
                var product = await _productsService.GetProduct((int)orderDetail.ProductID);
                OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel()
                {
                    DiscountMoney = orderDetail.DiscountMoney,
                    OrderID = orderDetail.OrderID,
                    ProductName = product.ProductName,
                    Quantity = orderDetail.Quantity,
                    Subtotal = orderDetail.Subtotal,
                    ProductID = product.ProductID,
                    Price = product.Price,
                    OrderDetailID = orderDetail.OrderID
                    
                };
                orderDetailViewModels.Add(orderDetailViewModel);
            }
            CheckOutBillViewModel checkOutView = new CheckOutBillViewModel();

            CheckOut checkOut = await _checkOutService.GetCheckOut((int)order.OrderID);
            if (checkOut != null)
            {
                Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);

                checkOutView.IdOrder = checkOut.IdOrder;
                checkOutView.IsReceived = checkOut.IsReceived;
                checkOutView.TotalPrice = order.TotalAmount;
                checkOutView.OrderDate = order.OrderDate;
                checkOutView.PhoneNumber = customer.PhoneNumber;
                checkOutView.Address = customer.Address;
                checkOutView.FirstName = customer.FirstName + " " + customer.LastName;
                checkOutView.Email = customer.Email;
                checkOutView.IsAccept = order.IsDone;
                checkOutView.IsReceived = checkOut.IsReceived;
                checkOutView.Note = checkOut.Note;
                checkOutView.orderDetails = orderDetailViewModels;
                checkOutView.PaymentStatus = order.PaidStatus;
            }

            return checkOutView;

        }
        //GetOrderDetailComplete
        [HttpGet]
        public async Task<IActionResult> GetOrderDetailComplete(int orderId)
        {


            var order = await _orderService.GetOrder(orderId);
            var orderDetails = await _orderDetailService.GetOrderDetailsByOrderId(orderId);
            List<OrderDetailViewModel> orderDetailViewModels = new List<OrderDetailViewModel>();
            foreach (var orderDetail in orderDetails)
            {
                var product = await _productsService.GetProduct((int)orderDetail.ProductID);
                OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel()
                {
                    DiscountMoney = orderDetail.DiscountMoney,
                    OrderID = orderDetail.OrderID,
                    ProductName = product.ProductName,
                    Quantity = orderDetail.Quantity,
                    Subtotal = orderDetail.Subtotal,
                    ProductID = product.ProductID,
                    Price = product.Price,
                    OrderDetailID = orderDetail.OrderID
                };
                orderDetailViewModels.Add(orderDetailViewModel);
            }
            CheckOutBillViewModel checkOutView = new CheckOutBillViewModel();

            CheckOut checkOut = await _checkOutService.GetCheckOut((int)order.OrderID);
            if (checkOut != null)
            {
                Customer customer = await _customerService.GetCustomer((int)checkOut.CustomerId);
                Employee employee = await _employeeService.GetEmployeeByAccountId((int)order.AccountId);
                checkOutView.IdOrder = checkOut.IdOrder;
                checkOutView.IsReceived = checkOut.IsReceived;
                checkOutView.TotalPrice = order.TotalAmount;
                checkOutView.OrderDate = order.OrderDate;
                checkOutView.PhoneNumber = customer.PhoneNumber;
                checkOutView.Address = customer.Address;
                checkOutView.FirstName = customer.FirstName + " " + customer.LastName;
                checkOutView.Email = customer.Email;
                checkOutView.IsAccept = order.IsDone;
                checkOutView.IsReceived = checkOut.IsReceived;
                checkOutView.Note = checkOut.Note;
                checkOutView.orderDetails = orderDetailViewModels;
                checkOutView.AccountID = order.AccountId;
                checkOutView.employeeName = employee.FirstName + " " + employee.LastName;

            }
            return Json(checkOutView);

        }



        public async Task<IActionResult> CheckOutBillByEmployess(int orderId,int accountId)
        { 
             Order order = await _orderService.GetOrder(orderId);
             order.AccountId = accountId;
             order.IsDone = true;
             await _orderService.UpdateOrder(order);
            //add report here
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GenerateBillPDFAsync(int orderId)
        {
            CheckOutBillViewModel checkOutView = await QueryOrderDetail(orderId);
            string htmlContent = GenerateHtmlBill(checkOutView);
            byte[] pdfBytes;
            using (var pdfStream = new MemoryStream())
            {
                PdfDocument pdfDoc = new PdfDocument(new PdfWriter(pdfStream));
                using (Document document = new Document(pdfDoc))
                {
                    ConverterProperties properties = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, properties);
                }
                pdfBytes = pdfStream.ToArray();
            }
            return File(pdfBytes, "application/pdf", "bill.pdf");
        }

        private string GenerateHtmlBill(CheckOutBillViewModel checkOutView)
        {
            string date = checkOutView.OrderDate.HasValue ? checkOutView.OrderDate.Value.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) : "";
            string htmlBill = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{
                        font-family: Tahoma, sans-serif;
                        font-size: 16px;
                        color: #333;
                    }}
                    h1, p, table {{
                        font-family: Tahoma, sans-serif;
                    }}
                    table {{
                        border-collapse: collapse;
                        width: 100%;
                        margin-bottom: 20px;
                    }}
                    th, td {{
                        border: 1px solid #ccc;
                        padding: 8px;
                    }}
                    th {{
                        background-color: #f5f5f5;
                    }}
                </style>
            </head>
            <body>
                <header>
                    <h1>Tho Bakery Shop</h1>
                    <p>Địa chỉ: 315 Bình Trung 2, Bình Thạnh Đông, Phú Tân, An Giang</p>
                    <p>Điện thoại: 0917 444 190</p>
                </header>
                <table>
                    <thead>
                        <tr>
                            <th>Mã hóa đơn</th>
                            <th>Tên khách hàng</th>
                            <th>Số điện thoại</th>
                            <th>Địa chỉ</th>
                            <th>Email</th>
                            <th>Ngày đặt hàng</th>
                            <th>Tổng giá</th>
                            <th>Ghi chú</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{checkOutView.IdOrder}</td>
                            <td>{checkOutView.FirstName}</td>
                            <td>{checkOutView.PhoneNumber}</td>
                            <td>{checkOutView.Address}</td>
                            <td>{checkOutView.Email}</td>
                            <td>{date}</td>
                            <td>{checkOutView.TotalPrice}</td>
                            <td>{checkOutView.Note}</td>
                        </tr>
                    </tbody>
                </table>
                <h2>Chi tiết sản phẩm:</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Tên sản phẩm</th>
                            <th>Đơn giá</th>
                            <th>Số lượng</th>
                            <th>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>";

                    foreach (var orderDetail in checkOutView.orderDetails)
                    {
                        htmlBill += $@"
                    <tr>
                        <td>{orderDetail.ProductName}</td>
                        <td>{orderDetail.Price}</td>
                        <td>{orderDetail.Quantity}</td>
                        <td>{orderDetail.Subtotal}</td>
                    </tr>";
                    }
                        if (checkOutView.PaymentStatus == true)
                        {
                            htmlBill += $@"
                    <tr>
                        <td>Thu hộ:</td>
                        <td></td>
                        <td></td>
                        <td> Đơn hàng đã được thanh toán </td>
                    </tr>";
                        }
                        else
                        {
                            htmlBill += $@"
                    <tr>
                        <td>Thu hộ:</td>
                        <td></td>
                        <td></td>
                        <td>{checkOutView.TotalPrice}</td>
                    </tr>";
                        }


            htmlBill += @"


                  </tbody>
                </table>
                <footer>
                    <p>Cảm ơn quý khách đã sử dụng dịch vụ của chúng tôi!</p>
                </footer>
            </body>
            </html>";


            return htmlBill;
        }
        public async Task<IActionResult> CheckOutBillDoneByEmployess(int orderId)// thêm cái set lại đã thanht oán ở đây
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                Order order = await _orderService.GetOrder(orderId);
                if (order.PaidStatus == false)
                {
                    order.PaidStatus = true;
                }
                await _orderService.UpdateOrder(order);
                CheckOut checkOut = await _checkOutService.GetCheckOut(orderId);
                checkOut.IsReceived = true;
                CheckOutBillViewModel checkOutBillViewModel = await QueryOrderDetail(orderId);
                try
                {
                    foreach (OrderDetailViewModel orderDetail in checkOutBillViewModel.orderDetails)
                    {
                        Rate rate = new Rate()
                        {
                            ProductId = (int)orderDetail.ProductID,
                            Star = 5,
                            StatusRate = true
                        };
                       await _rateService.InsertRate(rate);
                    }

                    await _checkOutService.UpdateCheckOut(checkOut);
                    scope.Complete();
                    return RedirectToAction("CheckOutCompleteBill");
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return NotFound();
                }
                
            }
        }
        public async Task<IActionResult> RemoveOrderByAdmin(int orderId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                string validationCode = "";
                string phoneNumber = "";
                try
                {
                    Order order = await _orderService.GetOrder(orderId);
                    CheckOut checkOut = await _checkOutService.GetCheckOut(orderId);

                    IQueryable<OrderDetail> orderDetails = await _orderDetailService.GetOrderDetailsByOrderId(orderId);

                    await _checkOutService.DeleteCheckOut(checkOut);
                    foreach (OrderDetail detail in orderDetails)
                    {
                        await _orderDetailService.DeleteOrderDetail(detail);
                    }
                    await _orderService.DeleteOrder(order);

                    scope.Complete();
                    return RedirectToAction("CheckOutCompleteBill", "DashBoard");
                   
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return NotFound();
                }
            }



        }
    }

}
