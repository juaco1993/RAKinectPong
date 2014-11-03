using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using Microsoft.Kinect;



namespace KinectPong
{

    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public static class variablesGlobales
    {
        static int _contador;
        public static int contador
        {
            get
            {
                return _contador;
            }
            set
            {
                _contador = value;
            }
        }
        static int _velocidad;
        public static int velocidad
        {
            get
            {
                return _velocidad;
            }
            set
            {
                _velocidad = value;
            }
        }
        static bool _tocoArr;
        public static bool tocoArr
        {
            get
            {
                return _tocoArr;
            }
            set
            {
                _tocoArr = value;
            }
        }
        static bool _tocoAba;
        public static bool tocoAba
        {
            get
            {
                return _tocoAba;
            }
            set
            {
                _tocoAba = value;
            }
        }
        static bool _tocoDer;
        public static bool tocoDer
        {
            get
            {
                return _tocoDer;
            }
            set
            {
                _tocoDer = value;
            }
        }
        static bool _tocoIzq;
        public static bool tocoIzq
        {
            get
            {
                return _tocoIzq;
            }
            set
            {
                _tocoIzq = value;
            }
        }
        static Thickness _margenPelota;
        public static Thickness margenPelota
        {
            get
            {
                return _margenPelota;
            }
            set
            {
                _margenPelota = value;
            }
        }
        public static double margenPelotaIzq
        {
            get
            {
                return _margenPelota.Left;
            }
            set
            {
                _margenPelota.Left= value;
            }
        }
        public static double margenPelotaArr
        {
            get
            {
                return _margenPelota.Top;
            }
            set
            {
                _margenPelota.Top = value;
            }
        }
    }

    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        private readonly Brush[] skeletonBrushes;

        private WriteableBitmap depthImageBitMap;
        private Int32Rect depthImageBitmapRect;
        private Int32 depthImageStride;
        private DepthImageFrame lastDepthFrame;

        private WriteableBitmap colorImageBitmap;
        private Int32Rect colorImageBitmapRect;
        private int colorImageStride;
        private byte[] colorImagePixelData;

        private Skeleton[] frameSkeletons;

        public MainWindow()
        {
            InitializeComponent();

            skeletonBrushes = new Brush[] { Brushes.Red };

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        public KinectSensor KinectDevice
        {

            get { return this.kinectDevice; }
            set
            {
                if (this.kinectDevice != value)
                {
                    //Uninitialize
                    if (this.kinectDevice != null)
                    {
                        this.kinectDevice.Stop();
                        this.kinectDevice.SkeletonFrameReady -= kinectDevice_SkeletonFrameReady;
                        this.kinectDevice.ColorFrameReady -= kinectDevice_ColorFrameReady;
                        this.kinectDevice.DepthFrameReady -= kinectDevice_DepthFrameReady;
                        this.kinectDevice.SkeletonStream.Disable();
                        this.kinectDevice.DepthStream.Disable();
                        this.kinectDevice.ColorStream.Disable();
                        this.frameSkeletons = null;
                    }

                    this.kinectDevice = value;

                    //Initialize
                    if (this.kinectDevice != null)
                    {
                        if (this.kinectDevice.Status == KinectStatus.Connected)
                        {
                            this.kinectDevice.SkeletonStream.Enable();
                            this.kinectDevice.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                            this.kinectDevice.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                            this.frameSkeletons = new Skeleton[this.kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.kinectDevice.SkeletonFrameReady += kinectDevice_SkeletonFrameReady;
                            this.kinectDevice.ColorFrameReady += kinectDevice_ColorFrameReady;
                            this.kinectDevice.DepthFrameReady += kinectDevice_DepthFrameReady;
                            this.kinectDevice.Start();

                            DepthImageStream depthStream = kinectDevice.DepthStream;
                            depthStream.Enable();

                            //Todo esto es la Imagen de profundidad
                            depthImageBitMap = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                            depthImageBitmapRect = new Int32Rect(0, 0, depthStream.FrameWidth, depthStream.FrameHeight);
                            depthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;

                            //Todo esto es la Imagen RGB
                            ColorImageStream colorStream = kinectDevice.ColorStream;
                            colorStream.Enable();
                            colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight,
                                                                                            96, 96, PixelFormats.Bgr32, null);
                            this.colorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                            this.colorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                            ColorImage.Source = this.colorImageBitmap;

                            DepthImage.Source = depthImageBitMap;
                        }
                    }
                }
            }
        }

        //Que hacer cuando nos viene un Frame de Profundidad
        void kinectDevice_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    short[] depthPixelDate = new short[depthFrame.PixelDataLength];
                    depthFrame.CopyPixelDataTo(depthPixelDate);
                    depthImageBitMap.WritePixels(depthImageBitmapRect, depthPixelDate, depthImageStride, 0);
                }
            }
        }

        //Que hacer cuando nos viene un Frame de Color
        void kinectDevice_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this.colorImageBitmap.WritePixels(this.colorImageBitmapRect, pixelData, this.colorImageStride, 0);
                }
            }
        }


        //Que hacer con cada Frame de Esqueleto
        void kinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Polyline figure;
                    Polyline cuadradoMano;
                    Brush userBrush;
                    SolidColorBrush brushJoaquin = new SolidColorBrush();
                    Skeleton skeleton;
                    Point ubicacionManoDer;
                    Point ubicacionManoIzq;
                    Point ubicacionCabeza;
                    ubicacionManoDer = new Point();
                    ubicacionManoIzq = new Point();
                    cuadradoMano = new Polyline();
                    

                    brushJoaquin.Color = Color.FromRgb(0, 255, 0);
                    cuadradoMano.StrokeThickness = 8;
                    cuadradoMano.Stroke = brushJoaquin;


                    LayoutRoot.Children.Clear();
                    frame.CopySkeletonDataTo(this.frameSkeletons);


                    for (int i = 0; i < this.frameSkeletons.Length; i++)
                    {
                        skeleton = this.frameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length];



                            //Dibujamos la cabeza y el cuerpo
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter
                                                                });
                            //LayoutRoot.Children.Add(figure);

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            //LayoutRoot.Children.Add(figure);

                            //Dibujamos la pierna Izquierda
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            //LayoutRoot.Children.Add(figure);

                            //Dibujamos la pierna Derecha
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            //LayoutRoot.Children.Add(figure);

                            //Dibujamos el brazo Izquierdo
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                            //LayoutRoot.Children.Add(figure);

                            //Dibujamos el Brazo Derecho
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                            //LayoutRoot.Children.Add(figure);

                            //Dibujamos el cuadrado de la mano
                            ubicacionManoDer = GetJointPoint(skeleton.Joints[JointType.HandRight]);
                            ubicacionManoIzq = GetJointPoint(skeleton.Joints[JointType.HandLeft]);
                            ubicacionCabeza = GetJointPoint(skeleton.Joints[JointType.Head]);

                            //cuadradoMano.Points.Add(ubicacionManoDer);
                            //ubicacionManoDer.X = ubicacionManoDer.X - 20;
                            //cuadradoMano.Points.Add(ubicacionManoDer);
                            //ubicacionManoDer.Y = ubicacionManoDer.Y + 20;
                            //cuadradoMano.Points.Add(ubicacionManoDer);
                            //ubicacionManoDer.X = ubicacionManoDer.X + 20;
                            //cuadradoMano.Points.Add(ubicacionManoDer);
                            //ubicacionManoDer.Y = ubicacionManoDer.Y - 20;
                            //cuadradoMano.Points.Add(ubicacionManoDer);

                            //Thickness margenPelota = pelota.Margin;
                            //margenImagen.Left = ubicacionManoDer.X - 220;
                            //margenImagen.Top = ubicacionManoDer.Y - 30;

                            //Thickness margenImagen2 = pbxCabeza.Margin;
                            //margenImagen2.Left = ubicacionCabeza.X - 30;
                            //margenImagen2.Top = ubicacionCabeza.Y - 30;

                            //Thickness margenImagen3 = elipseManoIzq.Margin;
                            //margenImagen3.Left = ubicacionManoIzq.X - 30;
                            //margenImagen3.Top = ubicacionManoIzq.Y - 30;


                            //POSICION DE LAS IMAGENES EN COORDENADAS DE LA PANTALLA (RELATIVAS AL MARGEN)
                            //pbxManoDer.Margin = margenImagen;
                            //pbxCabeza.Margin = margenImagen2;
                            //elipseManoIzq.Margin = margenImagen3;

                            //System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\1.wav");
                            //System.Media.SoundPlayer platillo = new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\Bateria\Kawai\rezocymbal.wav");
                            //System.Media.SoundPlayer guitarra1= new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\Bateria\guitarra1.wav");
                            //System.Media.SoundPlayer guitarra2 = new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\Bateria\guitarra2.wav");
                            //System.Media.SoundPlayer guitarra3 = new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\Bateria\guitarra3.wav");
                            //System.Media.SoundPlayer guitarra4 = new System.Media.SoundPlayer(@"C:\Users\Joaquin\Music\Nuevas\Bateria\guitarra4.wav");

                            //if (ubicacionManoDer.X > 640)
                            //    player.Play();
                            //if (ubicacionManoDer.Y < 0)
                            //    player.Stop();

                            //if ((((ubicacionManoDer.X >= 200) && (ubicacionManoDer.X <= 412)) && ((ubicacionManoDer.Y >= 326) && (ubicacionManoDer.X <= 481)))
                            //    || (((ubicacionManoIzq.X >= 200) && (ubicacionManoIzq.X <= 412)) && ((ubicacionManoIzq.Y >= 326) && (ubicacionManoIzq.X <= 481))))
                            //    platillo.Play();
                            //if (ubicacionManoDer.Y < 0)
                            //    platillo.Stop();
                            if ((ubicacionManoDer.Y <= pelota.Margin.Top + 50 && ubicacionManoDer.Y >= pelota.Margin.Top-50) && (ubicacionManoDer.X >= pelota.Margin.Left-50 && ubicacionManoDer.X <= pelota.Margin.Left+50)) 
                            {
                                variablesGlobales.tocoDer = true;
                                variablesGlobales.tocoIzq = false;
                                variablesGlobales.contador = variablesGlobales.contador + 1;
                                variablesGlobales.velocidad = variablesGlobales.velocidad + 2;
                                
                            }



                            //if (((margenImagen3.Left >= margenImagen.Left) && (margenImagen3.Right <= margenImagen.Right))
                            //    && ((margenImagen3.Bottom >= margenImagen.Bottom) && (margenImagen3.Top <= margenImagen.Top)))
                            //{
                            //pelota.Margin = variablesGlobales.margenPelota;
                            
                            if (pelota.Margin.Left >= ventana.Width - 70)
                            {
                                variablesGlobales.tocoDer = true;
                                variablesGlobales.tocoIzq = false;


                                variablesGlobales.margenPelotaIzq = 250;
                                variablesGlobales.margenPelotaArr = 190;
                                variablesGlobales.tocoDer = true;
                                variablesGlobales.tocoAba = true;
                                variablesGlobales.tocoIzq = false;
                                variablesGlobales.tocoArr = false;
                                variablesGlobales.contador = 0;
                                variablesGlobales.velocidad = 4;
                                
                            }
                            else if (pelota.Margin.Left <= 20)
                            {
                                variablesGlobales.tocoIzq = true;
                                variablesGlobales.tocoDer = false;
                            }

                            if (pelota.Margin.Top >= ventana.Height - 70)
                            {
                                variablesGlobales.tocoAba = true;
                                variablesGlobales.tocoArr = false;
                            }
                            else if (pelota.Margin.Top <= 20)
                            {
                                variablesGlobales.tocoArr = true;
                                variablesGlobales.tocoAba = false;
                            }




                            if (variablesGlobales.tocoArr) 
                            {
                                variablesGlobales.margenPelotaArr = pelota.Margin.Top + variablesGlobales.velocidad;
                                pelota.Margin = variablesGlobales.margenPelota;
                                variablesGlobales.tocoAba = false;

                            }
                            else if (variablesGlobales.tocoAba)
                            {
                                variablesGlobales.margenPelotaArr = pelota.Margin.Top - variablesGlobales.velocidad;
                                pelota.Margin = variablesGlobales.margenPelota;
                                variablesGlobales.tocoArr = false;

                            }
                            
                            if (variablesGlobales.tocoDer)
                            {
                                variablesGlobales.margenPelotaIzq = pelota.Margin.Left - variablesGlobales.velocidad;
                                pelota.Margin = variablesGlobales.margenPelota;
                                variablesGlobales.tocoIzq = false;

                            }
                            else if (variablesGlobales.tocoIzq)
                            {
                                variablesGlobales.margenPelotaIzq = pelota.Margin.Left + variablesGlobales.velocidad;
                                pelota.Margin = variablesGlobales.margenPelota;
                                variablesGlobales.tocoDer = false;

                            }

                            //valorMargen.Text = variablesGlobales.margenPelotaIzq + " " + variablesGlobales.margenPelotaArr;
                            valorMargen.Text = variablesGlobales.contador.ToString();



                            //}

                            //LayoutRoot.Children.Add(cuadradoMano);

                            //LayoutRoot.Children.Add(pbxCaritaFeliz);


                        }
                    }
                }
            }
        }

        //Esta funcion la llamamos para Dibujar las Lineas del Esqueleto, le pasamos el Esqueleto, el Pincel y los Joints
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));

            }

            return figure;
        }


        //Aqui Realizamos el mapeo (la correccion de coordenadas del esqueleto con las imagenes RGB o con la Profundidad)
        private Point GetJointPoint(Joint joint)
        {
            CoordinateMapper cm = new CoordinateMapper(kinectDevice);

            //DepthImagePoint point = cm.MapSkeletonPointToDepthPoint(joint.Position, this.KinectDevice.DepthStream.Format);
            ColorImagePoint point = cm.MapSkeletonPointToColorPoint(joint.Position, this.KinectDevice.ColorStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }

        private void ventana_Loaded(object sender, RoutedEventArgs e)
        {

            variablesGlobales.margenPelota = pelota.Margin;
            variablesGlobales.tocoDer = true;
            variablesGlobales.tocoAba = true;
            variablesGlobales.tocoIzq = false;
            variablesGlobales.tocoArr = false;
            variablesGlobales.contador = 0;
            variablesGlobales.velocidad = 4;
        }



    }
}
