using System.Windows;
using Autofac;

namespace BBPalCalc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IContainer Container { get; set; }

        public static IGlobals GetGlobal()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                return scope.Resolve<IGlobals>();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var builder = new ContainerBuilder();
            builder.RegisterType<Globals>().As<IGlobals>().SingleInstance();
            Container = builder.Build();
        }
    }
}
