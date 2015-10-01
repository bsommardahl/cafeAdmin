using System.Linq;
using Cafe.Data;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;

namespace Cafe.DailyReports
{
    public class DebitModule : NancyModule
    {
        public DebitModule()
        {
            Post["/debits/{debitId}/delete"] =
                o =>
                    {
                        var debitId = (string) o.debitId;

                        using (var dc = new CafeDataContext())
                        {
                            Debit debit = dc.Debits.First(x => x._id == debitId);
                            dc.Debits.DeleteOnSubmit(debit);
                            dc.SubmitChanges();

                            return new RedirectResponse(Request.Headers.Referrer);
                        }
                    };

            Post["/debits/{debitId}/type"] =
                o =>
                    {
                        var debitId = (string) o.debitId;
                        var input = this.Bind<DebitModifyInput>();

                        using (var dc = new CafeDataContext())
                        {
                            Debit debit = dc.Debits.First(x => x._id == debitId);
                            debit.Type = input.Type;
                            dc.SubmitChanges();

                            return new RedirectResponse(Request.Headers.Referrer);
                        }
                    };
        }
    }
}