using Microsoft.Maker.Firmata;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WRAWiFiLight
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        
        public  MainPage()
        {
            this.InitializeComponent();
            this.rgbSelector.Loaded += RgbSelector_Loaded;
        }

        private async void RgbSelector_Loaded(object sender, RoutedEventArgs e)
        {
        }

        RemoteDevice arduino;
        NetworkSerial netWorkSerial;
        UwpFirmata firmata;
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            ushort port = System.Convert.ToUInt16(this.txtPort.Text);

            firmata = new UwpFirmata();
            arduino = new RemoteDevice(firmata);

            netWorkSerial = new NetworkSerial(new Windows.Networking.HostName(this.txtIpAddress.Text), port);


            netWorkSerial.ConnectionEstablished += NetWorkSerial_ConnectionEstablished; ;
            netWorkSerial.ConnectionFailed += NetWorkSerial_ConnectionFailed; ;

            firmata.begin(netWorkSerial);
            netWorkSerial.begin(115200, SerialConfig.SERIAL_8N1);

            this.txtStatus.Text = "Connecting..";



        }

        private void NetWorkSerial_ConnectionFailed(string message)
        {
            this.txtStatus.Text = string.Format("Connection Failed. {0}", message);

        }

        private void NetWorkSerial_ConnectionEstablished()
        {
            this.txtStatus.Text = "Connected";

        }

        WriteableBitmap rgbGradient;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Setup the WriteableBitmap object from which the color at the touched point will be retrieved.
            RenderTargetBitmap rgbGradientTarget = new RenderTargetBitmap();
            
            await rgbGradientTarget.RenderAsync(rgbSelector, rgbGradientTarget.PixelWidth, rgbGradientTarget.PixelHeight);
            IBuffer pixelBuffer = await rgbGradientTarget.GetPixelsAsync();

            var width = rgbGradientTarget.PixelWidth;
            var height = rgbGradientTarget.PixelHeight;

            rgbGradient = await new WriteableBitmap(width, height).FromPixelBuffer(pixelBuffer, width, height);

        }


        /// <summary>
        /// Touch/Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rgbSelector_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var touchedPoint=e.GetCurrentPoint(this.rgbSelector);

            var scaledX = ((int)touchedPoint.Position.X / this.rgbSelector.Width) * this.rgbGradient.PixelWidth;
            var scaledY = ((int)touchedPoint.Position.Y / this.rgbSelector.Height) * this.rgbGradient.PixelHeight;

            Color color = rgbGradient.GetPixel((int)scaledX, (int)scaledY);

            var colorData = string.Format("{0},{1},{2}", color.R, color.G, color.B);

            if (firmata != null)
            {
                firmata.sendString(colorData);
                firmata.flush();
            }

            this.rgbText.Text = colorData;
            e.Handled = true;
        }



    }
}
