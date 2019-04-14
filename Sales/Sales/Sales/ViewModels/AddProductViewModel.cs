﻿

namespace Sales.ViewModels
{
    using System;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Plugin.Media;
    using Plugin.Media.Abstractions;
    using Sales.Common.Models;
    using Sales.Helpers;
    using Sales.Services;
    using Xamarin.Forms;   

    public class AddProductViewModel : BaseViewModel
    {
        #region Attribute

        private MediaFile file;

        private ImageSource imageSource;

        private ApiService apiService;

        private bool isRunning;

        private bool isEnabled;

        #endregion

        #region Properties

        public string Description { get; set; }

        public string Price { get; set; }

        public string Remarks { get; set; }
       

        public bool IsRunning
        {
            get { return this.isRunning; }
            set { this.SetValue(ref this.isRunning, value); }
            
        }

        public ImageSource ImageSource
        {
            get { return this.imageSource; }
            set { this.SetValue(ref this.imageSource, value); }

        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.SetValue(ref this.isEnabled, value); }

        }

        #endregion

        #region Constructors

        public AddProductViewModel()
        {
           this.apiService = new ApiService();
           this.IsEnabled = true;
            this.ImageSource = "NotImage";
        }


        #endregion

        #region Commands

        public ICommand ChangeImageCommand
        {
            get
            {
                return new RelayCommand(ChangeImage);
            }

        }

        private async void ChangeImage()
        {
            await CrossMedia.Current.Initialize();

            var source = await Application.Current.MainPage.DisplayActionSheet(
                Languages.ImageSource,
                Languages.Cancel,
                null,
                Languages.FromGallery,
                Languages.NewPicture);

            if (source == Languages.Cancel)
            {
                this.file = null;
                return;
            }

            if (source == Languages.NewPicture)
            {
                this.file = await CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions
                    {
                        Directory = "Sample",
                        Name = "test.jpg",
                        PhotoSize = PhotoSize.Small,
                    }
                );
            }
            else
            {
                this.file = await CrossMedia.Current.PickPhotoAsync();
            }

            if (this.file != null)
            {
                this.ImageSource = ImageSource.FromStream(() =>
                {
                    var stream = this.file.GetStream();
                    return stream;
                });
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(Save);
            }
           
        }

        private async void Save()
        {
            if (String.IsNullOrEmpty(this.Description))
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.DescriptionError,
                    Languages.Accept);
                return;
            }

            if (String.IsNullOrEmpty(this.Price))
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PriceError,
                    Languages.Accept);
                return;

            }

            var price = decimal.Parse(this.Price);

            if (price < 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PriceError,
                    Languages.Accept);
                return;

            }

            this.IsRunning = true;
            this.IsEnabled = false;

            var conecction = await this.apiService.CheckConnection();
            if (!conecction.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error, 
                    conecction.Message, 
                    Languages.Accept);
                return;
            }

            var product = new Product
            {
                Description = this.Description,
                Price = price,
                Remarks = this.Remarks,

            };

            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductController"].ToString();
            var response = await this.apiService.Post(url, prefix, controller, product);

            if (!response.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error, 
                    response.Message, 
                    Languages.Accept);
                return;
            }

            var newProduct = (Product)response.Result;
            var viewModel = ProductsViewModel.GetIntance();
            viewModel.Products.Add(newProduct);
            
            this.IsRunning = false;
            this.IsEnabled = true;
            await Application.Current.MainPage.Navigation.PopAsync();
        }


        #endregion
    }
}