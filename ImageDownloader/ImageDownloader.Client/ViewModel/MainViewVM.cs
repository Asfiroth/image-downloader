using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageDownloader.Models;
using ImageDownloader.Services.Contract;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageDownloader.Client.ViewModel
{
    public class MainViewVm : ViewModelBase
    {
        #region Members
        private ICommand _loadFileCommand;
        private ICommand _selectDestinationCommand;
        private ICommand _startCommand;
        private bool _canStart;
        private string _loadFileName;
        private string _destinationName;
        private ObservableCollection<Product> _products;
        private readonly IImageHandler _imageHandler;
        private bool _isBusy;
        private string _loadMessage;
        #endregion
        #region Properties
        public ICommand LoadFileCommand => _loadFileCommand ?? (_loadFileCommand = new RelayCommand(LoadFileExecute));

        public ICommand SelectDestinationCommand
            => _selectDestinationCommand ?? (_selectDestinationCommand = new RelayCommand(SelectDestinationExecute));

        public ICommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(StartLoadExecute));

        public bool CanStart
        {
            get { return _canStart; }
            set
            {
                _canStart = value;
                RaisePropertyChanged();
            }
        }

        public string LoadFileName
        {
            get { return GetFileOrFolderName(_loadFileName) ?? "Ingrese el archivo a cargar"; }
            set
            {
                _loadFileName = value;
                RaisePropertyChanged();
                CanStart = !string.IsNullOrEmpty(_loadFileName) && !string.IsNullOrEmpty(_destinationName);
            }
        }

        public string DestinationName
        {
            get { return GetFileOrFolderName(_destinationName) ?? "Carpeta de Destino"; }
            set
            {
                _destinationName = value;
                RaisePropertyChanged();
                CanStart = !string.IsNullOrEmpty(_loadFileName) && !string.IsNullOrEmpty(_destinationName);
            }
        }

        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                RaisePropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(); }
        }
        
        public string LoadMessage
        {
            get { return _loadMessage; }
            set { _loadMessage = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Contructor

        public MainViewVm(IImageHandler imageHandler)
        {
            if (IsInDesignMode) return;

            if (imageHandler == null)
                throw new ArgumentNullException(nameof(imageHandler));

            _imageHandler = imageHandler;
        }

        #endregion

        #region Command Methods

        private void LoadFileExecute()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                DefaultExt = "*.xlsx",
                Filter = "Libro de Excel (*.xlsx) | *.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            openFileDialog.ShowDialog();
            LoadFileName = string.IsNullOrEmpty(openFileDialog.FileName) ? string.Empty : openFileDialog.FileName;
        }

        private void SelectDestinationExecute()
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result == DialogResult.OK)
                DestinationName = fbd.SelectedPath;
        }

        private void StartLoadExecute()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += LoadLUrlAsync;
            worker.RunWorkerCompleted += WorkCompleted;
            worker.RunWorkerAsync();
        }

        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            var loadWorker = new BackgroundWorker();
            loadWorker.DoWork += DownloadImages;
            loadWorker.RunWorkerCompleted += FinalWork;
            loadWorker.RunWorkerAsync();
        }

        private void FinalWork(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
        }

        private void DownloadImages(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            LoadMessage = "Descargando Imagenes...";
            _imageHandler.DownloadImages(Products, _destinationName);
        }
        #endregion

        #region Private Methods

        private string GetFileOrFolderName(string path)
        {
            return string.IsNullOrEmpty(path) ? null : Path.GetFileName(path);
        }

        private void LoadLUrlAsync(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            LoadMessage = "Cargando Imagenes en memoria...";
            Products = new ObservableCollection<Product>(_imageHandler.LoadOnMemory(_loadFileName));
        }
        #endregion
    }
}