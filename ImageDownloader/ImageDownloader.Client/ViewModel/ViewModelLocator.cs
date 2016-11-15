using GalaSoft.MvvmLight.Ioc;
using ImageDownloader.Services.Contract;
using ImageDownloader.Services.Implementation;
using Microsoft.Practices.ServiceLocation;

namespace ImageDownloader.Client.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<IImageHandler, ImageHandler>();

            SimpleIoc.Default.Register<MainViewVm>();
        }

        public MainViewVm MainViewModel => ServiceLocator.Current.GetInstance<MainViewVm>();
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}