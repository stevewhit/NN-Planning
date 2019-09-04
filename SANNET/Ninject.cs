using Framework.Generic.EntityFramework;
using log4net;
using Ninject;
using Ninject.Modules;
using SANNET.Business.Repositories;
using SANNET.Business.Services;
using SANNET.DataModel;
using StockMarket.DataModel;

namespace QR.App
{
    public class NinjectBindings : NinjectModule
    {
        public override void Load()
        {
            var context = new EfContext(new SANNETContext());

            Bind<IEfRepository<Company>>().ToMethod(_ => new EfRepository<Company>(context));
            Bind<IEfRepository<Quote>>().ToMethod(_ => new EfRepository<Quote>(context));
            Bind<IEfRepository<Prediction>>().ToMethod(_ => new EfRepository<Prediction>(context));
            Bind<IEfRepository<NetworkConfiguration>>().ToMethod(_ => new EfRepository<NetworkConfiguration>(context));
            Bind<IDatasetRepository>().ToMethod(_ => new DatasetRepository(context));

            Bind<ICompanyService<Company>>().ToConstructor(_ => new CompanyService<Company>(Kernel.Get<IEfRepository<Company>>())).InThreadScope();
            Bind<IQuoteService<Quote>>().ToConstructor(_ => new QuoteService<Quote>(Kernel.Get<IEfRepository<Quote>>())).InThreadScope();
            Bind<IDatasetService>().ToConstructor(_ => new DatasetService(Kernel.Get<IDatasetRepository>())).InThreadScope();
            Bind<INetworkConfigurationService<NetworkConfiguration>>().ToConstructor(_ => new NetworkConfigurationService<NetworkConfiguration>(Kernel.Get<IEfRepository<NetworkConfiguration>>())).InThreadScope();
            Bind<IPredictionService<Prediction>>().ToConstructor(_ => new PredictionService<Prediction, Quote, Company, NetworkConfiguration>(Kernel.Get<IEfRepository<Prediction>>(), 
                                                                                                                                              Kernel.Get<IQuoteService<Quote>>(), 
                                                                                                                                              Kernel.Get<ICompanyService<Company>>(), 
                                                                                                                                              Kernel.Get<IDatasetService>(), 
                                                                                                                                              Kernel.Get<INetworkConfigurationService<NetworkConfiguration>>())).InThreadScope();
                        
            Bind<ILog>().ToMethod(_ => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));
        }
    }
}
