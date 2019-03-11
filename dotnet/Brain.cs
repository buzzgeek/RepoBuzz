using System;
//using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;


namespace BuzzNet
{
    /// <summary>
    /// Summary description for Brain.
    /// </summary>
    public class Brain : IDisposable
    {
        public class NeuronCluster
        {
            private Matrix<double>[] energyMatrix = new Matrix<double>[LAYERS]; // neuron contained 'energy'
            private Matrix<double>[] activationMatrix = new Matrix<double>[LAYERS]; // neuron 'energy'
            private Matrix<double>[] deltaMatrix = new Matrix<double>[LAYERS]; // all deltas used to calculate derivatives in gradiend descent/back propagation
            private Matrix<double>[] weightMatrix = new Matrix<double>[LAYERS]; // all weights per layer, per neuron to neuron fully connected
            private Matrix<double>[] biasMatrix = new Matrix<double>[LAYERS]; // all biases per layer, per neuron
            public double learningRate = Properties.Settings.Default.LearningRate;

            Random rand = null;

            public void Reset() { }

            public void Load(string urlNet)
            {
                try
                {

                    for (int i = 0; i < weightMatrix.Length; i++)
                    {
                        //weightMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.params", urlNet));
                        weightMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.w.{1}", urlNet, i), Compression.GZip);
                    }
                    for (int i = 0; i < biasMatrix.Length; i++)
                    {
                        //biasMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.params", urlNet));
                        biasMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.b.{1}", urlNet, i), Compression.GZip);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            public void Save(string urlNet)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(string.Format("{0}.params", urlNet));

                    int i = 0;
                    foreach (Matrix<double> w in weightMatrix)
                    {
                        MatrixMarketWriter.WriteMatrix<double>(sw, w);
                        MatrixMarketWriter.WriteMatrix<double>(string.Format("{0}.w.{1}", urlNet, i), w, Compression.GZip);
                        i++;
                    }
                    i = 0;
                    foreach (Matrix<double> b in biasMatrix)
                    {
                        MatrixMarketWriter.WriteMatrix<double>(sw, b);
                        MatrixMarketWriter.WriteMatrix<double>(string.Format("{0}.b.{1}", urlNet, i), b, Compression.GZip);
                        i++;
                    }
                    sw.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            public void Init(int seed)
            {
                rand = new Random(seed);

                // for each layer create neurons and their related matrices, not that the weight matrix is different, 
                // since each neuron of a given layer is fully connected to all neurons of the next layer
                for (int i = 0; i < LAYERS; i++)
                {
                    energyMatrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // no 'energy' or excitement yet
                    activationMatrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // no 'energy' or excitement yet

                    if (Properties.Settings.Default.InitialZeroParams)
                    {
                        weightMatrix[i] = Matrix<double>.Build.Dense(NEURONS, NEURONS, 0.00001); // almost zero weights, there is a problem otherwise during backpropagation not able 
                        biasMatrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // zero biases
                    }
                    else
                    {
                        weightMatrix[i] = Matrix<double>.Build.Random(NEURONS, NEURONS, seed); // random weights
                        weightMatrix[i] = weightMatrix[i].Map(InitRandomWeight); // -.5 to .5 seems to work a bit better ?!
                        biasMatrix[i] = Matrix<double>.Build.Random(NEURONS, 1, seed); // random biases
                    }
                    deltaMatrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // no backpropagation error's have been calculated yet, only one error per neuron as oposed to one error per weight
                }
            }

            public static void Clear(ref Matrix<double>[] matrix)
            {
                for (int i = 0; i < LAYERS; i++)
                {
                    matrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // no 'energy' or excitement yet
                }
            }

            public double Train(Matrix<double> inputMatrix, Matrix<double> expectedMatrix, ref double acc)
            {
                // generate output via forward propagation and calculate output neuron's errors
                double err = ForwardPropagation(inputMatrix, expectedMatrix, ref acc);

                // there is another aproach that might be better
                // first: calculate all costs of all training samples
                // second: iterate thru all samples again and perform back propagation, while averaging the error matrices in the end
                // with this approach the learning rate might be not required

                // ...and here is where the magic happens
                // adjust the network weights and biases via backward propagation
                BackwardPropagation(learningRate, expectedMatrix);

                return err;
            }

            private double CalculateErrorAndAccuracy(Matrix<double> inputMatrix, Matrix<double> expectedMatrix, ref double accuracy)
            {
                double res = 0.0;

                Matrix<double> differences = activationMatrix[LAYERS - 1].Subtract(expectedMatrix);
                Matrix<double> costs = differences.Map(Power2);

                IEnumerable<Tuple<int, int, double>> e = costs.EnumerateIndexed();
                long totalPix = 0;
                double acc = 0.0;
                foreach (Tuple<int, int, double> t in e)
                {
                    // calculate accumulated accuracy
                    acc += (1.0 - Math.Abs(differences[t.Item1, t.Item2]));
                    totalPix++;
                    // and total cost
                    res += Math.Abs(costs[t.Item1, t.Item2]);
                }

                accuracy = totalPix > 0 ? ((double)acc / (double)totalPix) * 100.0 : 0.0;

                return res;
            }

            private double Power2(double arg)
            {
                return Math.Pow(arg, 2.0);
            }

            private double InitRandomWeight(double arg)
            {
                return rand.NextDouble() - 0.5; // values -.5 to .5
            }

            public Matrix<double> Predict(Matrix<double> input)
            {
                NeuronCluster.Clear(ref this.activationMatrix);

                // input layer
                energyMatrix[0] = input.Clone();
                activationMatrix[0] = energyMatrix[0].Map<double>(Sigmoid);

                // propagate signals thru all layers
                for (int i = 1; i < LAYERS; i++)
                {
                    energyMatrix[i] = weightMatrix[i].Multiply(activationMatrix[i - 1]);
                    energyMatrix[i] = energyMatrix[i].Add(biasMatrix[i]);
                    activationMatrix[i] = energyMatrix[i].Map<double>(Sigmoid);
                }

                return activationMatrix[LAYERS - 1].Clone();
            }

            public double ForwardPropagation(Matrix<double> input, Matrix<double> expected, ref double accuracy)
            {
                try
                {
                    Predict(input);

                    return CalculateErrorAndAccuracy(input, expected, ref accuracy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return -1.0;
            }

            private void BackwardPropagation(double lr, Matrix<double> expected)
            {
                try
                {
                    if (LAYERS < 1)
                    {
                        throw new ArgumentOutOfRangeException("NumberOfLayers", "The NumberOfLayers defined in the applications user settings has to be greater than 1");
                    }
                    // see on YouTube 3Blue1Brown - YouTube - https://www.youtube.com/channel/UCYO_jab_esuFRV4b17AJtAw - very good videos on these type of dnns and linear algebra (vector matrix calculations)
                    // see http://neuralnetworksanddeeplearning.com/chap1.html for very good introduction and tutorial in python
                    // see https://sudeepraja.github.io/Neural/ on Backward propagation (excluding bias)

                    // ouput neuron's errors will be calculate in the ForwardPropagation function 
                    // calculate the output neurons delta's to reduce costs/errors

                    // activation[L] = sig(energy[L])
                    // error[L] = (activation[L] - expected)
                    // delta[L] = error • sig'(energy[L])
                    Matrix<double> primeEnergy = energyMatrix[LAYERS - 1].Map(SigmoidPrime);
                    Matrix<double> derivative = null;
                    Matrix<double> tmp = null;

                    deltaMatrix[LAYERS - 1] = activationMatrix[LAYERS - 1].Subtract(expected);
                    deltaMatrix[LAYERS - 1] = Matrix<double>.op_DotMultiply(deltaMatrix[LAYERS - 1], primeEnergy);

                    // calculate the remaining errors
                    for (int i = LAYERS - 2; i > 0; i--)
                    {
                        primeEnergy = energyMatrix[i].Map(SigmoidPrime);
                        deltaMatrix[i] = weightMatrix[i + 1].Transpose().Multiply(deltaMatrix[i + 1]);
                        deltaMatrix[i] = Matrix<double>.op_DotMultiply(deltaMatrix[i], primeEnergy);
                    }

                    // adjust weights and biases

                    for (int i = LAYERS - 1; i > 0; i--)
                    {
                        // derivative[i] = delta[i] * activation[i-1]T
                        derivative = deltaMatrix[i].Multiply(activationMatrix[i - 1].Transpose());
                        tmp = Matrix<double>.op_DotMultiply(weightMatrix[i], derivative).Multiply(lr);
                        // w[i] = w[i] - eta * w[i] • derivative[i]
                        weightMatrix[i] = weightMatrix[i].Subtract(tmp);
                        // b = b - (eta * derivative)
                        biasMatrix[i] = biasMatrix[i].Subtract(deltaMatrix[i].Multiply(lr));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #region constants

        private NeuronCluster cluster = new NeuronCluster();
        private const double TOLERANCE = 0.00001;
        private const int IMG_OFFSET_X = 10;
        private const int IMG_OFFSET_Y = 10;
        private static int LAYERS = Properties.Settings.Default.Layers;
        private static int IMG_SIZE = Properties.Settings.Default.ImgSize;
        private static int NEURONS = IMG_SIZE * IMG_SIZE;
        private const int OFFSET = 10; // not used
        private const int ORIGIN_X = 20;
        private const int ORIGIN_Y = 20;

        private int[] NEIGHBOURS = { -6, -5, -4, -1, +1, +4, +5, +6 };
        private int[] FAR_NEIGHBOURS = { -12, -11, -10, -9, -8, -7, -3, -2, +2, +3, +7, +8, +9, +10, +11, +12 };
        private static string[] urlTrainingImages = null;
        private static string[] urlExpectedImages = null;

        private double OUTPUT_THREASHOLD = 1.0;

        private double FAC = 0.5;
        private double NEIGHBOURS_FAC = 0.35;
        private double FAR_NEIGHBOURS_FAC = 0.15;
        private double totScore = 0.0;
        private double prevTotScore = double.MaxValue;
        private double smallestError = double.MaxValue;
        private double largestError = 0;
        private DateTime prevStepDateTime = DateTime.Now;
        private string estimateRemainingTime = "";

        private Bitmap guiInput = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap guiOutput = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap guiExpected = new Bitmap(IMG_SIZE, IMG_SIZE);

        private Bitmap trainIn = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap trainOut = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap trainExp = new Bitmap(IMG_SIZE, IMG_SIZE);

        private Bitmap saveIn = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap saveOut = new Bitmap(IMG_SIZE, IMG_SIZE);
        private Bitmap saveExp = new Bitmap(IMG_SIZE, IMG_SIZE);

        private static Bitmap resTemplateImage = new Bitmap(IMG_SIZE, IMG_SIZE);

        private bool brainActive = false;
        private int imageIndex = 0;
        private int trainImageIndex = 0;
        private static Bitmap canvas = new Bitmap(2000, 4000);
        private Thread BrainProcessorThread = null;
        private bool isActive = false;
        //private int prevImageIndex = -1;
        private object brainLock = new object();
        private static Brush brushBlack = new SolidBrush(Color.Black);
        private static Brush brushWhite = new SolidBrush(Color.White);
        private static Brush brushGreen = new SolidBrush(Color.Green);
        private static Brush brushRed = new SolidBrush(Color.DarkRed);
        private static Pen penWhite = new Pen(Color.White);
        private static Font fontSnaptshot = new Font(FontFamily.GenericMonospace, 24, FontStyle.Bold);
        private List<Point> circleCoordinates = new List<Point>();
        private long iteration = 0;
        private int tile = 0;
        private long totalNumberOfIterations = Properties.Settings.Default.NumberOfIterations;
        private long numberOfBestScores = 0;
        private long numberOfWorstScores = 0;
        private long numberOfDeterioratingScores = 0;
        private long numberOfImprovingScores = 0;
        private double totalAccuracy = 0f;
        private double bestAccuracy = 0f;
        private double worstAccuracy = 100f;
        private string comment = "Setup...";

        private static Random randMatrix = new Random();
        private TestControl parent = null;
        private bool terminate = false;
        private bool isSaving = false;
        private System.Threading.AutoResetEvent saveEvent = new AutoResetEvent(false);

        public void BeforeTerminate()
        {
            terminate = true;
            comment = "Saving hyper parameters, please wait...";
        }

        public void Terminate()
        {
            SaveSnapshot();
        }

        public NeuronCluster Cluster
        {
            get { return cluster; }
            set { cluster = value; }
        }

        ////Signal Activation Function - also see https://en.wikipedia.org/wiki/Activation_function
        //private static double Softsign(double value)
        //{
        //    double res = 0.0;
        //    try
        //    {
        //        // softsign seems a bit cheaper : -1 to 1
        //        // min effective value: -999999999999999
        //        // max effective value: 999999999999999
        //        res = value / (1.0 + Math.Abs(value));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return res;
        //}

        // Derivative of signal activation function useful for backpropagation
        private static double SigmoidPrime(double value)
        {
            double res = 0.0;
            try
            {
                // actual sigmoid/soft step function : 0 to 1
                // min effective value: -700
                // max effective value: 30
                double s = Sigmoid(value);

                res = s * (1 - s);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return res;
        }

        //Signal Activation Function
        private static double Sigmoid(double value)
        {
            double res = 0.0;
            try
            {
                // actual sigmoid/soft step function : 0 to 1
                // min effective value: -700
                // max effective value: 30
                res = 1.0 / (1.0 + Math.Exp((-1 * value)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return res;
        }

        private static double init(int arg1, int arg2)
        {
            return randMatrix.NextDouble();
        }

        #endregion constants
        #region members

        // input interface to brain 
        ArrayList synIn = new ArrayList();

        // output interface to brain
        ArrayList synOut = new ArrayList();

        System.Random rand = null;

        int x_offset = OFFSET;
        int y_offset = OFFSET;

        #endregion members

        #region properties

        public Neuron[,] BrainCells { get; set; } = new Neuron[LAYERS, NEURONS];

        [JsonIgnore]
        public bool Active
        {
            get { return brainActive; }
        }

        [JsonIgnore]
        public int NumLayers
        {
            get { return LAYERS; }
        }

        [JsonIgnore]
        public int NumNeurons
        {
            get { return NEURONS; }
        }
        #endregion properties

        #region constructor

        public Brain(TestControl parent, int seed, int width, int height)
        {
            this.parent = parent;
            x_offset = (int)width / (int)(LAYERS * 1.2);
            y_offset = 10;

            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Length < 2)
            {
                rand = new Random(seed);
                cluster.Init(seed);
            }
            else
            {
                cluster.Load(cmdArgs[1]);
            }

            brainActive = true;

            string urlOut = string.Format("{0}\\result", Properties.Settings.Default.UrlTrainingImages);
            if (!Directory.Exists(urlOut))
            {
                Directory.CreateDirectory(urlOut);
            }

        }
        #endregion constructor

        #region destructor
        ~Brain()
        {
        }
        #endregion destructor

        private void Start()
        {
            if (isActive) return;
            isActive = true;

            if (Properties.Settings.Default.UseMatrixBrain)
            {
                BrainProcessorThread = new Thread(new ThreadStart(this.TrainMatrixBrain));
                BrainProcessorThread.Start();
            }
        }

        private void PerformTraining()
        {
            if (isActive) return;

            isActive = true;

            if (Properties.Settings.Default.UseMatrixBrain)
            {
                BrainProcessorThread = new Thread(new ThreadStart(this.TrainMatrixBrain));
            }
            //else
            //{
            //    BrainProcessorThread = new Thread(new ThreadStart(this.TrainBrain));
            //}

            BrainProcessorThread.Start();
        }


        public static void Setup()
        {
            if (Directory.Exists(string.Format("{0}\\input", Properties.Settings.Default.UrlTrainingImages)) &&
                Directory.Exists(string.Format("{0}\\expected", Properties.Settings.Default.UrlTrainingImages)))
            {
                urlTrainingImages = Directory.GetFiles(string.Format("{0}\\input", Properties.Settings.Default.UrlTrainingImages), Properties.Settings.Default.ImageExt);
                urlExpectedImages = Directory.GetFiles(string.Format("{0}\\expected", Properties.Settings.Default.UrlTrainingImages), Properties.Settings.Default.ImageExt);
            }
            else
            {
                urlTrainingImages = Directory.GetFiles(Properties.Settings.Default.UrlTrainingImages, Properties.Settings.Default.ImageExt);
            }

            using (Graphics graphicsHandle = Graphics.FromImage(resTemplateImage))
            {
                graphicsHandle.FillRectangle(brushBlack, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE));
                graphicsHandle.DrawEllipse(penWhite, new Rectangle(0, 0, IMG_SIZE - 1, IMG_SIZE - 1));
            }

            using (Graphics graphicsHandle = Graphics.FromImage(canvas))
            {
                graphicsHandle.FillRectangle(brushBlack, new Rectangle(new Point(0, 0), canvas.Size));
            }
        }

        public void Activate()
        {

            if (Properties.Settings.Default.Train)
            {
                PerformTraining();
            }
            return;
        }

        public void Draw(Graphics gHandle)
        {
            gHandle.FillRectangle(brushBlack, new Rectangle(new Point(0, 0), canvas.Size));

            if (terminate)
            {
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    SolidBrush brush = new SolidBrush(Color.White);
                    Font font = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
                    if (comment.Length > 0)
                        g.DrawString(comment
                            , font
                            , brush,
                            new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + ((IMG_SIZE / 2) - (font.Height / 2))));
                    font.Dispose();
                    brush.Dispose();
                    gHandle.DrawImage(Brain.canvas, new Point(0, 0));
                }
                return;
            }

            if (imageIndex >= urlTrainingImages.Length)
            {
                return;
            }

            if (!terminate)
            {
                using (Graphics g = Graphics.FromImage(canvas))
                {

                    int row_offset = imageIndex * (IMG_SIZE + 1);

                    if (Properties.Settings.Default.DrawNet)
                    {
                        for (int l = 0; l < LAYERS; ++l)
                        {
                            for (int n = 0; n < NEURONS; ++n)
                            {
                                BrainCells[l, n].Draw(g);
                            }
                        }
                    }
                    else
                    {
                        row_offset = 0;
                        g.FillRectangle(brushBlack, new Rectangle(new Point(0, 0), canvas.Size));
                    }

                    if (Properties.Settings.Default.EnablePreview)
                    {
                        lock (brainLock)
                        {
                            if (comment.Length == 0 && urlTrainingImages != null && urlTrainingImages.Length > 0)
                            {
                                g.DrawImage(guiExpected, new Point((IMG_OFFSET_X + IMG_SIZE + IMG_SIZE + 2), IMG_OFFSET_Y + row_offset));
                                g.DrawImage(guiInput, new Point(IMG_OFFSET_X, IMG_OFFSET_Y + row_offset));
                                g.DrawImage(guiOutput, new Point(IMG_OFFSET_X + IMG_SIZE + 1, IMG_OFFSET_Y + row_offset));
                            }
                        }
                    }
                    SolidBrush brush = new SolidBrush(Color.White);
                    Font font = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
                    if (comment.Length > 0)
                        g.DrawString(comment
                            , font
                            , brush,
                            new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + ((IMG_SIZE / 2) - (font.Height / 2))));
                    else if (imageIndex < urlTrainingImages.Length)
                        g.DrawString(string.Format("{0,11}{1,20}\n" +
                            "{2,11}{3,20}\n" +
                            "{4,11}{5,20:#0.0000000000}\n" +
                            "{6,11}{7,20:#0.0000000000}\n" +
                            "{8,11}{9,20:#0.0000000000}\n" +
                            "{10,11}{11,20:#0.0000000000}\n" +
                            "{12,11}{13,20}\n" +
                            "{14,11}{15,20:#0}\n" +
                            "{16,11}{17,20:#0}\n" +
                            "{18,11}{19,20:#0}\n" +
                            "{20,11}{21,20:#0}\n" +
                            "{22,11}{23,20:#0.0000000000}\n" +
                            "{24,11}{25,20:#0.0000000000}\n" +
                            "{26,11}{27,20:#0.0000000000}\n" +
                            "{28,11}{29,20}\n",
                            "Tile#:",
                            string.Format("({0})", tile),
                            "TrainStep:",
                            string.Format("({0}/{1})", iteration, totalNumberOfIterations),
                            "LR:",
                            cluster.learningRate,
                            "Score:",
                            totScore,
                            "Best:",
                            smallestError,
                            "Worst:",
                            largestError,
                            "Index:",
                            string.Format("({0}/{1})", imageIndex + 1 + trainImageIndex, urlTrainingImages.Length),
                            "B#:",
                            numberOfBestScores,
                            "W#:",
                            numberOfWorstScores,
                            "D#:",
                            numberOfDeterioratingScores,
                            "I#:",
                            numberOfImprovingScores,
                            "ACC:",
                            totalAccuracy,
                            "BestACC:",
                            bestAccuracy,
                            "WorstACC:",
                            worstAccuracy,
                            "EstRemDur:",
                            estimateRemainingTime)
                            , font
                            , brush,
                            new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + IMG_SIZE + 4));
                    font.Dispose();
                    brush.Dispose();
                }
            }

            gHandle.DrawImage(Brain.canvas, new Point(0, 0));
        }

        private void TrainMatrixBrain()
        {
            comment = "training...";

            //// set initial scores and best weights
            totScore = double.MaxValue;
            totalAccuracy = 0.0;
            iteration = 0;
            while (iteration < totalNumberOfIterations && !terminate)
            {
                prevTotScore = totScore;
                totScore = ProcessImagesViaMatrix(ref totalAccuracy);

                if (totScore <= Properties.Settings.Default.RelevantScoreDelta)
                {
                    // we found the perfect net for the model
                    break;
                }

                if (bestAccuracy < totalAccuracy)
                {
                    bestAccuracy = totalAccuracy;
                }

                if (worstAccuracy > totalAccuracy)
                {
                    worstAccuracy = totalAccuracy;
                }

                if ((totScore - prevTotScore) >= Properties.Settings.Default.RelevantScoreDelta)
                {
                    numberOfDeterioratingScores++;
                }
                else
                {
                    numberOfImprovingScores++;
                }

                if ((totScore - largestError) >= Properties.Settings.Default.RelevantScoreDelta)
                {
                    largestError = totScore;
                }

                if ((smallestError - totScore) >= Properties.Settings.Default.RelevantScoreDelta)
                {
                    smallestError = totScore;
                    numberOfBestScores++;
                }

                parent.AddDataPoint(totScore, totalAccuracy, (ulong)iteration + 1);
                if (parent != null && (iteration % Properties.Settings.Default.Snapshot == 0) && !terminate)
                {
#if debug_single_tile
                    // do nothing - dunno if ! (negation) worx but the #else will
#else
                    SaveSnapshot();
#endif
                }

                iteration++;
                double stepDuration = (DateTime.Now - prevStepDateTime).TotalSeconds;
                prevStepDateTime = DateTime.Now;

                double estTotDuration = (totalNumberOfIterations - iteration) * stepDuration;
                TimeSpan estTotTimeSpan = TimeSpan.FromSeconds(estTotDuration);
                estimateRemainingTime = String.Format("{0:d\\.hh\\:mm\\:ss}", estTotTimeSpan); // shorter format
            }

            SaveSnapshot();
        }

        private void MatrixToImage(Matrix<double> m, ref Bitmap image)
        {
            IEnumerable<Tuple<int, int, double>> e = m.EnumerateIndexed();
            foreach (Tuple<int, int, double> t in e)
            {
                int x = t.Item1 / IMG_SIZE;
                int y = t.Item1 % IMG_SIZE;

                int alpha = 255;
                Color color = Color.White;
                if (t.Item3 <= Properties.Settings.Default.PositiveResult)
                {
                    double pcnt = Math.Abs((t.Item3 / (Properties.Settings.Default.PositiveResult)));
                    alpha = (int)(pcnt * 100);
                    color = Color.Green;
                }
                else
                {
                    alpha = 255;
                }

                Color colPixel = Color.FromArgb(alpha > 255 ? 255 : alpha, color);
                image.SetPixel(x, y, colPixel);
            }
        }

        private Matrix<double> BinaryImageToMatrix(Bitmap image)
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(NEURONS, 1);
            for (int x = 0; x < IMG_SIZE; x++)
            {
                for (int y = 0; y < IMG_SIZE; y++)
                {
                    Color px = image.GetPixel(x, y);
                    matrix[(x * IMG_SIZE) + y, 0] = px.R > 0 ? 1.0 : 0.0;
                }
            }

            return matrix;
        }


        private Matrix<double> ImageToMatrix(Bitmap image)
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(NEURONS, 1);
            for (int x = 0; x < IMG_SIZE; x++)
            {
                for (int y = 0; y < IMG_SIZE; y++)
                {
                    Color px = image.GetPixel(x, y);
                    matrix[(x * IMG_SIZE) + y, 0] = (double)(px.B + px.G + px.R) / (3.0 * 255.0);
                }
            }
            return matrix;
        }

        private double CalculateNeuronScore(Neuron neuron, List<int>[] input, List<int>[] expected)
        {
            double score = 0.0;
            for (int i = 0; i < input.Length; i++)
            {
                bool signal = neuron.Peek(input[i]);

                if (signal && expected[i][neuron.Index] == 1 || !signal && expected[i][neuron.Index] == 0)
                {
                    score += signal ? Properties.Settings.Default.ScoreXDelta : Properties.Settings.Default.ScoreDelta;
                }
                else
                {
                    score -= !signal ? Properties.Settings.Default.ScoreXDelta : Properties.Settings.Default.ScoreDelta;
                }
            }

            neuron.Score = score;

            return score;
        }

        private double ProcessImagesViaMatrix(ref double accuracy)
        {
            string timeToken = DateTime.Now.ToString("yyyyMMddhhmmssfff");
            double err = 0.0;
            double acc = 0.0;
            double totError = 0.0;
            int totNumOfTiles = 0;
            comment = "";

            int inputIndex = 0;
            imageIndex = 0;


            while (inputIndex < urlTrainingImages.Length && !terminate)
            {
                Bitmap image = new Bitmap(urlTrainingImages[inputIndex]);
                Bitmap expImage = urlExpectedImages != null ? new Bitmap(urlExpectedImages[inputIndex]) : null;

                int columns = image.Width / IMG_SIZE;
                int rows = image.Height / IMG_SIZE;

#if debug_single_tile
                columns = 1;
                rows = 1;
#endif
                acc = 0.0;
                err = 0;
                int randR = 0;
                int randC = 0;
                tile = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        randR = rand != null ? rand.Next(rows) : r;
                        randC = rand != null ? rand.Next(columns) : c;

                        if (expImage != null)
                        {
                            if (Properties.Settings.Default.EnablePreview)
                            {
                                lock (brainLock)
                                {
                                    using (Graphics g = Graphics.FromImage(guiInput))
                                    {
                                        g.DrawImage(image, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(randR * IMG_SIZE, randC * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                                    }
                                    using (Graphics g = Graphics.FromImage(guiExpected))
                                    {
                                        g.DrawImage(expImage, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(randR * IMG_SIZE, randC * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                                    }
                                }
                            }
                            using (Graphics g = Graphics.FromImage(trainIn))
                            {
                                g.DrawImage(image, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(randR * IMG_SIZE, randC * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                            }
                            using (Graphics g = Graphics.FromImage(trainExp))
                            {
                                g.DrawImage(expImage, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(randR * IMG_SIZE, randC * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                            }
                        }

                        Matrix<double> inputMatrix = ImageToMatrix(trainIn); // this is the original gray image
                        Matrix<double> expectedMatrix = ImageToMatrix(trainExp);

                        double tileAcc = 0.0;
                        err = cluster.Train(inputMatrix, expectedMatrix, ref tileAcc);
                        acc += tileAcc;
                        totError += err;
                        totNumOfTiles++;
                        accuracy = acc / (double)totNumOfTiles;

                        // this is totally not clear, but the accuracy here is actually totalAccuracy which is globaly used and has an effect on best and worst accuracy
                        // need to refactor to clean this. Probably should use Properties here to make things cleaner
                        if (bestAccuracy < accuracy)
                        {
                            bestAccuracy = accuracy;
                        }
                        if (worstAccuracy > accuracy)
                        {
                            worstAccuracy = accuracy;
                        }
                        if (Properties.Settings.Default.EnablePreview)
                        {
                            lock (brainLock)
                            {
                                Matrix<double> res = cluster.Predict(inputMatrix);
                                MatrixToImage(res, ref guiOutput);
                            }
                        }
                        tile++;
                        if (terminate) break;
                    }
                }

                if (image != null) image.Dispose();
                if (expImage != null) expImage.Dispose();

                inputIndex++;
                if ((inputIndex % Properties.Settings.Default.ChangeAfterIterations) == 0)
                {
                    cluster.learningRate = cluster.learningRate * Properties.Settings.Default.DeltaChangeBase;
                }
            }
            imageIndex = 0;

            // set return values accuracy and error
            return totError;
        }

        //private void AsyncSaveSnapshot()
        //{
        //    if (isSaving) return;
        //    isSaving = true;
        //    Thread thread = new Thread(SaveSnapshot);
        //    thread.Start();
        //    saveEvent.WaitOne();
        //    isSaving = false;
        //}

        private void SaveSnapshot()
        {
            if (isSaving) return;
            isSaving = true;

            try
            {
                string timeToken = DateTime.Now.ToString("yyyyMMddhhmmssfff");

                int inputIndex = 0;
                int tileIndex = 0;

                comment = "Saving snapshots....";

                if (!Directory.Exists(string.Format("{0}\\result", Properties.Settings.Default.UrlTrainingImages)))
                {
                    Directory.CreateDirectory(string.Format("{0}\\result", Properties.Settings.Default.UrlTrainingImages));
                }
                if (!Directory.Exists(string.Format("{0}\\result\\{1}", Properties.Settings.Default.UrlTrainingImages, timeToken)))
                {
                    Directory.CreateDirectory(string.Format("{0}\\result\\{1}", Properties.Settings.Default.UrlTrainingImages, timeToken));
                }

                while (inputIndex < urlTrainingImages.Length)
                {
                    Bitmap input = new Bitmap(urlTrainingImages[inputIndex]);
                    Bitmap expected = urlExpectedImages != null ? new Bitmap(urlExpectedImages[inputIndex]) : null;
                    double err = 0.0;
                    double acc = 0.0;
                    double totErr = 0.0;
                    double totAcc = 0.0;
                    int columns = input.Width / IMG_SIZE;
                    int rows = input.Height / IMG_SIZE;

                    Bitmap prediction = new Bitmap(columns * IMG_SIZE, rows * IMG_SIZE);

                    // we can probably do better than this 
                    tileIndex = 0;
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            if (expected != null)
                            {
                                using (Graphics g = Graphics.FromImage(saveIn))
                                {
                                    g.DrawImage(input, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(r * IMG_SIZE, c * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                                }
                                using (Graphics g = Graphics.FromImage(saveExp))
                                {
                                    g.DrawImage(expected, new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), new Rectangle(r * IMG_SIZE, c * IMG_SIZE, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                                }
                            }

                            comment = String.Format("tile#{0} prediction...", tileIndex);

                            Matrix<double> expectedMatrix = ImageToMatrix(saveExp);
                            Matrix<double> inputMatrix = ImageToMatrix(saveIn);
                            err = cluster.ForwardPropagation(inputMatrix, expectedMatrix, ref acc);
                            Matrix<double> outputMatrix = cluster.Predict(inputMatrix);
                            MatrixToImage(outputMatrix, ref saveOut);

                            using (Graphics g = Graphics.FromImage(prediction))
                            {
                                g.DrawImage(saveOut, new Rectangle(r * IMG_SIZE, c * IMG_SIZE, IMG_SIZE, IMG_SIZE), new Rectangle(0, 0, IMG_SIZE, IMG_SIZE), GraphicsUnit.Pixel);
                            }

                            if (Properties.Settings.Default.PersistSnapshotImageTiles)
                            {
                                SaveImageTiles(timeToken, err, acc, saveIn, saveOut, saveExp, tileIndex);
                            }

                            tileIndex++;
                            totErr += err;
                            totAcc += acc;
                        }
                    }

                    totAcc = tileIndex > 0f ? totAcc / tileIndex : 0f;
                    using (Graphics g = Graphics.FromImage(prediction))
                    {
                        g.DrawString(string.Format("A:{0:000.000000} S:{1:0.0000}", totAcc, totScore), fontSnaptshot, (totAcc / 100.0 < Properties.Settings.Default.PositiveResult) ? Brain.brushRed : Brain.brushGreen, new Point(10, IMG_SIZE));
                    }

                    string urlOutput = string.Format("{0}\\result\\{1}\\prediction_{2:0.00}.png", Properties.Settings.Default.UrlTrainingImages, timeToken, totAcc);

                    prediction.Save(urlOutput, System.Drawing.Imaging.ImageFormat.Png);
                    prediction.Dispose();


                    input.Dispose();
                    expected.Dispose();

                    inputIndex++;
                }

                Save(string.Format("{0}\\result\\{1}\\{2}", Properties.Settings.Default.UrlTrainingImages, timeToken, Properties.Settings.Default.UrlDefaultBrain));
            }
            catch (Exception) {
                //TODO 
            }
            saveEvent.Set();
            isSaving = false;
        }

        private void SaveImageTiles(string timeToken, double score, double accurary, Bitmap input, Bitmap output, Bitmap expected, int index)
        {
            string urlOutput = string.Format("{0}\\result\\{1}\\{2:000000}.png", Properties.Settings.Default.UrlTrainingImages, timeToken, index);

            Bitmap result = new Bitmap(IMG_SIZE + IMG_SIZE + IMG_SIZE + 8, IMG_SIZE + 20);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(input, new Point(0, 0));
                g.DrawImage(output, new Point(IMG_SIZE + 2, 0));
                g.DrawImage(expected, new Point((IMG_SIZE + IMG_SIZE + 4), 0));
                g.DrawString(string.Format("A:{0:000.000000} S:{1:0.0000}", accurary, score), SystemFonts.DefaultFont, Brain.brushGreen, new Point(0, IMG_SIZE));
            }

            result.Save(urlOutput, System.Drawing.Imaging.ImageFormat.Png);
            result.Dispose();
        }

        public void Rethink()
        {
            foreach (Neuron neuron in BrainCells)
            {
                neuron.Rethink();
            }
        }


        public void Wipeout()
        {
            foreach (Neuron neuron in BrainCells)
            {
                neuron.Wipeout();
            }
        }

        public void Rewire()
        {
            Wipeout();
            synIn.Clear();
            synOut.Clear();

            ConnectInputInterface(true);
            if (Properties.Settings.Default.UseImageBrain)
            {
                MouldImageBrain();
            }
            else
            {
                MouldBrain();
            }
        }

#warning the Mate method has not been implemented!
        public Brain Mate(Brain mate)
        {
            throw new System.NotImplementedException("Need to implement mating");
        }

        public void Shrink(int max, int rate)
        {

            ResetNeurons();

            for (int i = 0; i < max; ++i)
            {
                int layerPos = rand.Next(NumLayers);
                int neuronPos = rand.Next(NumNeurons);
                Neuron neuron = BrainCells[layerPos, neuronPos];

                if (rand.Next(rate) == 0)
                {
                    neuron.Shrink();
                }
            }

            CleanupNeurons();

            foreach (Synapse synapse in synIn)
            {
                synapse.Receiver.RemoveSynapseConnection(synapse);
            }

            synIn.Clear();
            synOut.Clear();

            ConnectInputInterface(false);
        }

#warning the Grow method has not been implemented!
        public void Grow()
        {
            throw new System.NotImplementedException("Need to implement growth");
        }

        public void Reset()
        {
            ResetNeurons();
        }

        public void Save(string urlSettings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.None); // indented is easier to handle by notepad++. none requires less hd space
                StreamWriter sw = new StreamWriter(urlSettings);
                sw.Write(json);
                sw.Close();
                cluster.Save(urlSettings);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Load(string urlSettings)
        {
            try
            {
                Brain brain;
                StreamReader sr = File.OpenText(urlSettings);
                string data = sr.ReadToEnd();
                sr.Close();
                brain = JsonConvert.DeserializeObject<Brain>(data);
                cluster.Load(urlSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        protected void ResetNeurons()
        {
            foreach (Neuron neuron in BrainCells)
            {
                neuron.Reset();
            }
        }

        protected void CleanupNeurons()
        {
            bool res = false;
            do
            {
                res = false;
                foreach (Neuron neuron in BrainCells)
                {
                    res |= neuron.Cleanup();
                }
                ResetNeurons();
            } while (res);
        }

        public void CreateGrayMatter()
        {
            int x = ORIGIN_X;
            int y = ORIGIN_Y;
            for (int i = 0; i < LAYERS; ++i)
            {
                y = ORIGIN_Y;
                for (int j = 0; j < NEURONS; ++j)
                {
                    //brainCells[i,j] = new Neuron(i, j, new Point(x,y), rand.Next());
                    BrainCells[i, j] = new Neuron(i, j, new Point(x, y), 42);
                    if (i == 0)
                        BrainCells[i, j].Threashhold = 0.0; // input neurons have no threashold, for now
                    else if (i == LAYERS - 1) // output neurons all have the same threashold
                        BrainCells[i, j].Threashhold = OUTPUT_THREASHOLD;
                    else
                        BrainCells[i, j].Threashhold = rand.NextDouble() * (double)rand.Next(10);
                    y += y_offset;
                }
                x += x_offset;
            }
        }

        protected void ConnectInputInterface(bool force)
        {
            for (int i = 0; i < NEURONS; ++i)
            {
                // connect input interface
                if (force || BrainCells[0, i].AxonsCount > 0)
                {
                    double weight = rand.NextDouble();
                    weight = weight > 0.2 ? weight : 0.2;
                    if (Properties.Settings.Default.UseImageBrain)
                        weight = 1.0;
                    Synapse syn = BrainCells[0, i].CreateSynapseConnection(ESynapsisType.Activator, weight);
                    synIn.Add(syn);
                }
            }
        }

        protected void MouldImageBrain()
        {
            // for each layer
            for (int i = 0; i < LAYERS - 1; ++i)
            {

                // create random connections to next layer
                for (int j = 0; j < NEURONS; ++j)
                {
                    Neuron sender = BrainCells[i, j];

                    // point-to-point
                    Neuron receiver = BrainCells[i + 1, j];
                    Synapse syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, FAC);
                    sender.AddAxonConnection(syn);

                    // point-to-nearest-neighbor
                    foreach (int h in NEIGHBOURS)
                    {
                        int pos = j + h;
                        if (pos >= 0 && pos < NEURONS)
                        {
                            receiver = BrainCells[i + 1, pos];
                            syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, NEIGHBOURS_FAC);
                            sender.AddAxonConnection(syn);
                        }
                    }

                    // point-to-furthest-neighbour
                    foreach (int q in FAR_NEIGHBOURS)
                    {
                        int pos = j + q;
                        if (pos >= 0 && pos < NEURONS)
                        {
                            receiver = BrainCells[i + 1, pos];
                            syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, FAR_NEIGHBOURS_FAC);
                            sender.AddAxonConnection(syn);
                        }
                    }
                }
            }
        }

        protected void MouldBrain()
        {
            // for each layer
            for (int i = 0; i < LAYERS - 1; ++i)
            {
                // create random connections to next layer
                for (int j = 0; j < NEURONS; ++j)
                {
                    Neuron sender = BrainCells[i, j];

                    // if the sending neuron is active
                    if (sender.SynapsesCount > 0)
                    {
                        //int maxCon = (i == (LAYERS-1) ? NEURONS : rand.Next(1, NEURONS+1));
                        int maxCon = (i == (LAYERS - 1) ? NEURONS : rand.Next(1, 9));
                        for (int c = 0; c < maxCon; ++c)
                        {
                            int pos = rand.Next(NEURONS);
                            Neuron receiver = BrainCells[i + 1, pos];

                            // but connect only once to a given receiver
                            if (!receiver.IsConnected(sender))
                            {
                                double weight = rand.NextDouble();
                                ESynapsisType type = rand.Next(2) > 0 ? ESynapsisType.Activator : ESynapsisType.Inhibitor;
                                Synapse syn = receiver.CreateSynapseConnection(type, weight);

                                sender.AddAxonConnection(syn);
                            }
                        }
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!Properties.Settings.Default.UseMatrixBrain)
            {
                for (int i = 0; i < LAYERS; ++i)
                {
                    for (int j = 0; j < NEURONS; ++j)
                    {
                        BrainCells[i, j].Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
