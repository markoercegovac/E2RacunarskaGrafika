// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Marko Ercegovac</author>
// <author>Marko Ercegovac RA18-2016</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Cameras;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;

using SharpGL.Enumerations;
using System.Linq;
using System.Windows.Threading;

namespace AssimpSample
{

    public enum TextureBlendingMode
    {
        Modulate,
        Decal,
        Replace,
        Blend
    };
    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi
        //visina stuba
        private double lightHeight=1;
        private double light = 1;
        private double radius = 1;
        int counter = 0;

        public OpenGL gl=new OpenGL();

        /*TEXTURE*/
        //Identifikatori tekstura
        private enum TextureObjects {Floor=0,ContainerWall,Gold,Container};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//Image//TexturesCom_Roads0172_1_M.jpg", "..//..//Image//brick.jpg", "..//..//Image//gold.jpg","..//..//Image//ferro enferrujado 1.jpg" };

        /*ANIMATION*/
        private float truckPosition;
        private float containerHeight;
        private float containerPosition=0;
        private float containerVerticalRotation;
        private float containerHorisontalRotation;
        private float rotation2=1;
        private Boolean isAnimation = false;
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private DispatcherTimer timer3;
        private DispatcherTimer timer4;
        private DispatcherTimer timer5;
        private double speed;

        private TextureBlendingMode m_selectedMode = TextureBlendingMode.Replace;

        //Atrubuti koji uticu na ponasanje kamere
        private LookAtCamera lookAtCamera;

        //Attributes for color
        private ShadeModel shadeModel;

        // Atributi koji uticu na ponasanje FPS kamere

        private float walkSpeed = 0.1f;
        float mouseSpeed = 0.005f;
        double horizontalAngle = 0f;
        double verticalAngle = 0.0f;

        //Pomocni vektori preko kojih definisemo lookAt funkciju
        private Vertex direction;
        private Vertex right;
        private Vertex up;

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;
        public AssimpScene m2_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 3000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        public AssimpScene Scene2
        {
            get { return m2_scene; }
            set { m2_scene = value; }
        }

        public TextureBlendingMode SelectedMode
        {
            get { return m_selectedMode; }
            set
            {
                m_selectedMode = value;
            }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public LookAtCamera LookAtCamera { get => lookAtCamera; set => lookAtCamera = value; }
        public double LightHeight { get => lightHeight; set => lightHeight = value; }
        public double Light { get => light; set => light = value; }
        public double Radius { get => radius; set => radius = value; }
        public double Speed { get => speed; set => speed = value; }
        public bool IsAnimation { get => isAnimation; set => isAnimation = value; }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, "caixa de lixo.3ds", gl);
            this.m2_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
            this.m_textures = new uint[m_textureCount];
            
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
           
            //MATERIALS

            // Model sencenja na flat (konstantno)
            //prva kontrolna tacka
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);


            //druga kontrolna tacka
            //osvetlenje
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            //gl.Enable(OpenGL.GL_TEXTURE_2D);

            SetupLighting(gl);
            gl.Enable(OpenGL.GL_TEXTURE_2D);






            //slika za teksturu

            // Ucitaj slike i kreiraj teksture i nista ne pitaj
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                
                gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }


           


            // Model sencenja na flat (konstantno)



            
           
           

            m2_scene.LoadScene();
            m2_scene.Initialize();
            m_scene.LoadScene();
            m_scene.Initialize();
            




        }

        public void StartAnimation()
        {
            rotation2 = 0;
            containerHeight = 0;
            truckPosition = 0;
            containerPosition = 0;
            containerVerticalRotation = 0;
            IsAnimation = true;
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(10);
            timer2.Tick += new EventHandler(this.UpdateAnimation2);
            timer2.Start();



            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(2);
            timer1.Tick += new EventHandler(this.UpdateAnimation1);
            timer1.Start();

            timer3 = new DispatcherTimer();
            timer3.Interval = TimeSpan.FromMilliseconds(10);
            timer3.Tick += new EventHandler(this.UpdateAnimation3);


            timer4 = new DispatcherTimer();
            timer4.Interval = TimeSpan.FromMilliseconds(10);
            timer4.Tick += new EventHandler(this.UpdateAnimation4);

            timer5 = new DispatcherTimer();
            timer5.Interval = TimeSpan.FromMilliseconds(1000);
            timer5.Tick += new EventHandler(this.UpdateAnimation5);
        }

        /*ANIMATION*/
        public void UpdateAnimation1(object sender, EventArgs e)
        {
            if (containerHeight > 200)
            {
                timer1.Stop();
                
            }
            containerHeight += 2;
            
        }

        public void UpdateAnimation2(object sender, EventArgs e)
        {
            
            truckPosition -= -5f;
            if (truckPosition > 900 )
            {
                timer2.Stop();
                
                timer3.Start();
            }
            
        }

        public void UpdateAnimation3(object sender, EventArgs e)
        {
            containerPosition -= 5f;
            if (containerPosition <-300)
            {
                timer3.Stop();
                timer4.Start();
            }
            
            
        }
        public void UpdateAnimation4(object sender,EventArgs e)
        {
            containerVerticalRotation -= 90f;

            timer5.Start();
            timer4.Stop();
            
            
        }

        public void UpdateAnimation5(object sender, EventArgs e)
        {
            rotation2 -= 5f*(float)speed;
            if (rotation2 < -80)
            {
                timer5.Stop();
                IsAnimation = false;
            }
            


            

        }

      
        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.FrontFace(OpenGL.GL_CCW);

            

            gl.PushMatrix();

            gl.LookAt(2000, 0, -3300, -3700,200,-3700, 0, 100, 0);
            gl.Translate(0.0f, -100.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            

            //podloga
            gl.PushMatrix();
            
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Floor]);
            gl.Color(1, 1, 1);
            gl.Normal(0, 1, 0);
            gl.Begin(OpenGL.GL_QUADS);
            changeMode(false);
            gl.TexCoord(0f, 1f);
            gl.Vertex(-1200.0f,0, -1000.0f);
            gl.TexCoord(0, 0);
            gl.Vertex(-1200.0f,0, 500.0f);
            gl.TexCoord(1, 0f);
            gl.Vertex(1200.0f,0, 500.0f);
            gl.TexCoord(1f, 1f);
            gl.Vertex(1200.0f,0, -1000.0f);
            gl.End();
            gl.PopMatrix();

            

            gl.PushMatrix();
            gl.Translate(0f, containerHeight, containerPosition);
            gl.Rotate(0, containerVerticalRotation, 0);
            gl.Rotate(rotation2, 0, 0);
            m_scene.Draw();
            
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-200.0f+truckPosition, 0f, -300f);
            
            m2_scene.Draw();
            gl.PopMatrix();



            //zid1
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.ContainerWall]);
            gl.Normal(1, 0, 0);
            gl.Color(0.4f, 0f, 0f);
            gl.Translate(0f,100f,-100f);
            gl.Scale(100f, 100f, 10f);
            

            Cube c = new Cube();
            c.Render(gl, RenderMode.Render);
            
            gl.PopMatrix();

            

            //zid2
            gl.PushMatrix();
            gl.Normal(-1, 0, 0);
            gl.Color(0.4f, 0f, 0f);
            gl.Translate(0f, 100f, 100f);
            gl.Scale(100f, 100f, 10f);
            Cube c2 = new Cube();
            c2.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //zid3
            gl.PushMatrix();
            gl.Normal(0, 0, 1);
            gl.Color(0.4f, 0f, 0f);
            gl.Translate(110f, 100f, 0f);
            gl.Scale(10f, 100f, 120f);
            Cube c3 = new Cube();
            c3.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //zid4
            gl.PushMatrix();
            
            gl.Color(0.4f, 0f, 0f);
            gl.Translate(-110f, 100f, 0f);
            gl.Scale(10f, 100f, 120f);
            Cube c4 = new Cube();
            c4.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //bandera
            gl.PushMatrix();
            gl.Rotate(-90, 0, 0);
            
            gl.Translate(150f, 0, 0);
            gl.Color(255, 213, 135);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Gold]);

            
            Cylinder cil = new Cylinder();
            cil.TopRadius = Radius*10;
            changeMode(true);
            cil.Height = lightHeight*500;
            cil.QuadricDrawStyle = DrawStyle.Fill;
            cil.TextureCoords = true;

            cil.BaseRadius = Radius*10;
            cil.CreateInContext(gl);
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            
            
            gl.PopMatrix();

            //svetlo

            gl.PushMatrix();
            
            gl.Color(1, 1f, 0f);
            gl.Translate(150f, 500*Light, 0);
            gl.Scale(30f, 30, 30f);
            
            Cube c6 = new Cube();
            
            
            c6.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PopMatrix();
            if (gl.RenderContextProvider != null)
            {
                gl.PushMatrix();

            
                gl.Translate(400f, -500.0f, -2000.0f);
                gl.Scale(40f, 40f, 40f);
            
                gl.DrawText3D("Helevetica", 25f, 1f, 0.1f, "Predmet:Racunarska grafika");
                gl.Translate(-10, -1f, 0);
                gl.DrawText3D("Helevetica", 25f, 1f, 0.1f, "Sk.god:2018/19.");
                gl.Translate(-6, -1f, 0);
                gl.DrawText3D("Helevetica", 25f, 1f, 0.1f, "Ime:Marko");
                gl.Translate(-6, -1f, 0);
                gl.DrawText3D("Helevetica", 25f, 1f, 0.1f, "Prezime:Ercegovac");
                gl.Translate(-6, -1f, 0);
                gl.DrawText3D("Helevetica", 25f, 1f, 0.1f, "Sifrazad: 1.1");

                gl.PopMatrix();
            }
            // Oznaci kraj iscrtavanja
            gl.Flush();

            
        }

        public void DrawAgain()
        {
            Draw(this.gl);


        }

        

        //podesavanje osvetljenja
        private void SetupLighting(OpenGL gl)
        {

            float[] global_ambient = new float[] { 0f, 0f, 0f, 1f };

            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);


            //tackasti
           

            
            float[] light0pos = new float[] { 150f, 500f, 1f };
        
            float[] light0ambient = new float[] { 1f, 1f, 1f,1f};
            float[] light0diffuse = new float[] { 1f, 1f, 1f,1f };
            float[] light0specular = new float[] { 1f, 1f, 1f, 1f };

            float[] light1pos = new float[] { 0f, 300f, 0f };

            float[] light1ambient = new float[] { 0.8f, 0f, 0f, 1f };
            float[] light1diffuse = new float[] { 0.8f, 0f, 0f, 1f };
            float[] light1specular = new float[] { 0.8f, 0f, 0f, 1f };

            float[] smer = { 0.0f, -1.0f, 0.0f };



            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT,light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE,light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR,light0specular);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR,light0specular);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 128f);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);

            
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT1);
            gl.Enable(OpenGL.GL_LIGHT0);





           








        }
        public void changeMode(bool isLight)
        {
            if (isLight)
            {
                this.gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            }
            this.gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);



        }
        


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.1f, -50f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            
            //gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
                
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
