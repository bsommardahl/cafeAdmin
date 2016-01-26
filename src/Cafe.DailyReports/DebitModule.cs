using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Cafe.Data;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;

namespace Cafe.DailyReports
{
    public class DebitModule:NancyModule
    {
        readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"].ToString();

        public DebitModule()
        {
            Post["/debits/{debitId}/type"] = 
                o =>
                    {
                        var debitId = (string) o.debitId;
                        var input = this.Bind<DebitModifyInput>();

                        using (var dc = new CafeDataContext(connectionString))
                        {
                            try
                            {
                                var debit = dc.Debits.First(x => x._id == debitId);
                                debit.Type = input.Type;
                                dc.SubmitChanges();
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                            return new RedirectResponse(this.Request.Headers.Referrer);
                        }
                        
                    };
        }
    }
}