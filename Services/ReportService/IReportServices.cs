using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortal.Services.ReportService
{
    public interface IReportServices
    {
        ICustomerReportServices customerReportServices();
        IVendorReportServices vendorReportServices();
    }
}