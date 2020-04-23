using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;
using System.Windows.Threading;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Garbage"), "F908_FTW.3ds", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.Width, (int)openGLControl.Height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_world.IsAnimation)
            {


                switch (e.Key)
                {
                    case Key.F9: this.Close(); break;
                    case Key.W:
                        if (m_world.RotationX > 0f)
                        {
                            m_world.RotationX -= 5.0f;
                        }
                        break;

                    case Key.S:
                        if (m_world.RotationX < 80f)
                        {
                            m_world.RotationX += 5.0f;

                        }
                        break;
                    case Key.A: m_world.RotationY -= 5.0f; break;
                    case Key.D: m_world.RotationY += 5.0f; break;
                    case Key.C:
                        m_world.StartAnimation();
                        break;
                    case Key.Add: m_world.SceneDistance -= 700.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 700.0f; break;
                    case Key.F2:
                        OpenFileDialog opfModel = new OpenFileDialog();
                        bool result = (bool)opfModel.ShowDialog();
                        if (result)
                        {

                            try
                            {
                                World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                                m_world.Dispose();
                                m_world = newWorld;
                                m_world.Initialize(openGLControl.OpenGL);
                            }
                            catch (Exception exp)
                            {
                                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK);
                            }
                        }
                        break;
                }
            }
        }

        private void ScalingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!m_world.IsAnimation) {
                m_world.LightHeight = this.scalingSlider.Value;
                m_world.Light = this.scalingSlider.Value;
                m_world.DrawAgain();
            }
            
          
        }

        private void ScalingBottomRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!m_world.IsAnimation)
            {
                m_world.Radius = this.scalingBottomRadius.Value;
                m_world.DrawAgain();
            }
            
        }

        private void GarbageThrowing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_world.Speed = this.garbageThrowing.Value;
            m_world.DrawAgain();

        }
    }
}
