using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortal.Services.ReportService
{
    public class ReportService : IReportServices
    {
        public IConfiguration _configuration { get; set; }
        public ReportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ICustomerReportServices customerReportServices()
        {
            return new CustomerReportServices(_configuration);
        }

        public IVendorReportServices vendorReportServices()
        {
            return new VendorReportServices(_configuration);
        }
    }
}