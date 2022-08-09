//#define faulty_calculation
using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;

/// <summary>
/// Copyright (c) 2019 Buzz-Barry Struck
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </summary>

namespace BuzzNet
{
    public class BrainGUI
    {
        private const int ORIGIN_X = 20;
        private const int ORIGIN_Y = 20;
        private const int IMG_OFFSET_X = 10;
        private const int IMG_OFFSET_Y = 10;

        private static int LAYERS = Properties.Settings.Default.Layers;
        private static int TILE_SIZE = Properties.Settings.Default.ImageTileSize;
        private static readonly int NEURONS = TILE_SIZE * TILE_SIZE;

        public static readonly Brush brushBlack = new SolidBrush(Color.Black);
        public static readonly Brush brushWhite = new SolidBrush(Color.White);
        public static readonly Brush brushGreen = new SolidBrush(Color.Green);
        public static readonly Brush brushRed = new SolidBrush(Color.DarkRed);
        public static readonly Pen penWhite = new Pen(Color.White);

        public static Font fontSnaptshot = new Font(FontFamily.GenericMonospace, 24, FontStyle.Bold);
        public static readonly Bitmap canvas = new Bitmap(2000, 4000);
        private static readonly Bitmap resTemplateImage = new Bitmap(TILE_SIZE, TILE_SIZE);

        private static Bitmap guiInput = new Bitmap(TILE_SIZE, TILE_SIZE);
        private static Bitmap guiOutput = new Bitmap(TILE_SIZE, TILE_SIZE);
        private static Bitmap guiExpected = new Bitmap(TILE_SIZE, TILE_SIZE);

        public static Bitmap GuiInput { get => guiInput; set => guiInput = value; }
        public static Bitmap GuiOutput { get => guiOutput; set => guiOutput = value; }
        public static Bitmap GuiExpected { get => guiExpected; set => guiExpected = value; }

        public static void Setup()
        {
            using (Graphics graphicsHandle = Graphics.FromImage(resTemplateImage))
            {
                graphicsHandle.FillRectangle(brushBlack, new Rectangle(0, 0, TILE_SIZE, TILE_SIZE));
                graphicsHandle.DrawEllipse(penWhite, new Rectangle(0, 0, TILE_SIZE - 1, TILE_SIZE - 1));
            }

            using (Graphics graphicsHandle = Graphics.FromImage(canvas))
            {
                graphicsHandle.FillRectangle(brushBlack, new Rectangle(new Point(0, 0), canvas.Size));
            }
        }

        public static void Draw(Graphics gHandle, Brain b)
        {
            gHandle.FillRectangle(brushBlack, new Rectangle(new Point(0, 0), canvas.Size));

            //if (false)
            //{
            //    using (Graphics g = Graphics.FromImage(canvas))
            //    {
            //        SolidBrush brush = new SolidBrush(Color.White);
            //        Font font = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
            //        if (b.comment.Length > 0)
            //            g.DrawString(b.comment,
            //                font,
            //                brush,
            //                new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + ((TILE_SIZE / 2) - (font.Height / 2))));
            //        font.Dispose();
            //        brush.Dispose();
            //        gHandle.DrawImage(canvas, new Point(0, 0));
            //    }
            //    return;
            //}

            if (b.imageIndex >= Brain.urlTrainingImages.Length)
            {
                return;
            }

            if (b.Active)
            {
                using (Graphics g = Graphics.FromImage(canvas))
                {

                    int row_offset = b.imageIndex * (TILE_SIZE + 1);

                    if (Properties.Settings.Default.DrawNet)
                    {
                        for (int l = 0; l < LAYERS; ++l)
                        {
                            for (int n = 0; n < NEURONS; ++n)
                            {
                                NeuronGUI.Draw(g, b.BrainCells[l, n]);
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
                        lock (b.brainLock)
                        {
                            if (b.comment.Length == 0 && Brain.urlTrainingImages != null && Brain.urlTrainingImages.Length > 0)
                            {
                                g.DrawImage(GuiExpected, new Point((IMG_OFFSET_X + TILE_SIZE + TILE_SIZE + 2), IMG_OFFSET_Y + row_offset));
                                g.DrawImage(GuiInput, new Point(IMG_OFFSET_X, IMG_OFFSET_Y + row_offset));
                                g.DrawImage(GuiOutput, new Point(IMG_OFFSET_X + TILE_SIZE + 1, IMG_OFFSET_Y + row_offset));
                            }
                        }
                    }

                    SolidBrush brush = new SolidBrush(Color.White);
                    Font font = new Font(FontFamily.GenericMonospace, 10, FontStyle.Regular);
                    if (b.comment.Length > 0)
                        g.DrawString(b.comment,
                            font,
                            brush,
                            new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + ((TILE_SIZE / 2) - (font.Height / 2))));
                    else if (b.imageIndex < Brain.urlTrainingImages.Length)
                        g.DrawString(b.cluster.metrics.ToString(),
                            font,
                            brush,
                            new Point((IMG_OFFSET_X + 4), IMG_OFFSET_Y + TILE_SIZE + 4));
                    font.Dispose();
                    brush.Dispose();
                }
            }

            gHandle.DrawImage(canvas, new Point(0, 0));
        }

        internal static void Apply(int r, int c, Bitmap image, Bitmap expImage)
        {
            using (Graphics g = Graphics.FromImage(guiInput))
            {
                g.DrawImage(image, new Rectangle(0, 0, TILE_SIZE, TILE_SIZE), new Rectangle(r * TILE_SIZE, c * TILE_SIZE, TILE_SIZE, TILE_SIZE), GraphicsUnit.Pixel);
            }
            using (Graphics g = Graphics.FromImage(guiExpected))
            {
                g.DrawImage(expImage, new Rectangle(0, 0, TILE_SIZE, TILE_SIZE), new Rectangle(r * TILE_SIZE, c * TILE_SIZE, TILE_SIZE, TILE_SIZE), GraphicsUnit.Pixel);
            }
        }

        internal static void ApplyOutputMatrix(Matrix<double> res)
        {
            Brain.MatrixToImage(res, ref guiOutput);
        }
    }

    public interface IBrain
    {
        void Activate();
        void Terminate();
        void Save(string urlSettings);
        void Load(string urlSettings);
        void CreateGrayMatter();
        void Rethink();
        void Wipeout();
        void Shrink(int max, int rate);
        void Reset();
    }
    /// <summary>
    /// The Brain class contains an implementation of a classical dnn via the cluster member as well as an object oriented approach.
    /// via the brainCells member. Initially the dnn cluster is trained on a set of training images, inorder to pre/initialize the weights and biases
    /// with the help of forward- and backward propagation.
    /// Once the dnn has reached a satisfactory state, the object oriented brain will be initialized with the weights and biases. The intend is
    /// to use this brain to further increase the accuracy of the results, by using a completely different approach to classic dnn math. The 
    /// object oriented approach has not been implemented yet, so this is still under construction. However, the classical dnn implementation seems to 
    /// provide sufficient results. I also have provided a python implementation using keras and tensorflow, in order to validate my results. To me
    /// the comparism comes up with similar results, so I am satisfied with my dnn implementation.
    /// </summary>
    public class Brain : IBrain, IShutdown
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Brain));

        #region internal interfaces and classes

        /// <summary>
        /// A 'classic' deep neuronal network interface
        /// see on YouTube 3Blue1Brown - YouTube - https://www.youtube.com/channel/UCYO_jab_esuFRV4b17AJtAw - very good videos on these type of dnns and linear algebra (vector matrix calculations)
        /// see http://neuralnetworksanddeeplearning.com/chap1.html for very good introduction and tutorial in python
        /// see https://sudeepraja.github.io/Neural/ on Backward propagation (excluding bias)
        /// </summary>
        public interface IDNN
        {
            double LearningRate { get; set; }
            void Init(int seed);
            void Load(string urlNet);
            void Save(string urlNet);
            double Train(Matrix<double> inputMatrix, Matrix<double> expectedMatrix, ref double acc);
            double ForwardPropagation(Matrix<double> input, Matrix<double> expected, ref double accuracy);
            Matrix<double> Predict(Matrix<double> input);
        }

        /// <summary>
        /// A 'classic' deep neuronal network implementation using forward- and backwardpropagation including a whole lot of linear algebra 
        /// </summary>
        public class DNN : IDNN
        {
            #region members

            internal Brain brain = null;
            private Matrix<double>[] energyMatrix = new Matrix<double>[LAYERS]; // neuron contained 'energy'
            private Matrix<double>[] activationMatrix = new Matrix<double>[LAYERS]; // neuron 'energy'
            private Matrix<double>[] deltaMatrix = new Matrix<double>[LAYERS]; // all deltas used to calculate derivatives in gradiend descent/back propagation
            private Matrix<double>[] weightMatrix = new Matrix<double>[LAYERS]; // all weights per layer, per neuron to neuron fully connected
            private Matrix<double>[] biasMatrix = new Matrix<double>[LAYERS]; // all biases per layer, per neuron
            private Random rand = null;

            public Metrics metrics = null;

            #endregion members

            #region properties

            public double LearningRate { get; set; } = Properties.Settings.Default.LearningRate;

            #endregion properties

            #region constructor

            public DNN(Brain parent)
            {
                this.brain = parent;
                this.metrics = new Metrics(this);
            }

            #endregion constructor

            #region IDNN

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

            public void Load(string urlNet)
            {
                try
                {

                    for (int i = 0; i < weightMatrix.Length; i++)
                    {
                        weightMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.w.{1}", urlNet, i), Compression.GZip);
                    }
                    for (int i = 0; i < biasMatrix.Length; i++)
                    {
                        biasMatrix[i] = MatrixMarketReader.ReadMatrix<double>(string.Format("{0}.b.{1}", urlNet, i), Compression.GZip);
                    }

                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
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
                    log.Error(ex);
                }
            }

            public double Train(Matrix<double> inputMatrix, Matrix<double> expectedMatrix, ref double acc)
            {
                // generate output via forward propagation and calculate output neuron's errors
                double err = ForwardPropagation(inputMatrix, expectedMatrix, ref acc);

                // there is another approach that might be better
                // first: calculate all costs of all training samples
                // second: iterate thru all samples again and perform back propagation, while averaging the error matrices in the end
                // with this approach the learning rate might be not required

                // ...and here is where the magic happens
                // adjust the network weights and biases via backward propagation
                BackwardPropagation(LearningRate, expectedMatrix);

                return err;
            }

            public Matrix<double> Predict(Matrix<double> input)
            {
                try
                {
                    for (int i = 0; i < LAYERS; i++)
                    {
                        activationMatrix[i] = Matrix<double>.Build.Dense(NEURONS, 1, 0.0); // no 'energy' or excitement yet
                    }

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
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
                return null;
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
                    log.Error(ex.Message);
                }
                return -1.0;
            }

            #endregion IDNN

            #region private methods

            private void BackwardPropagation(double lr, Matrix<double> expected)
            {
                try
                {
                    if (LAYERS < 1)
                    {
                        throw new ArgumentOutOfRangeException("NumberOfLayers", "The NumberOfLayers defined in the applications user settings has to be greater than 1");
                    }
                    // neuron's energies and activations have been calculated in the ForwardPropagation function 
                    // calculate the neurons delta's to reduce costs/errors

                    // activation[L] = sig(energy[L])
                    // error[L] = (activation[L] - expected)
                    // delta[L] = error • sig'(energy[L]) #:hadamard product
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

#if faulty_calculation
                        // w[i] = w[i] - eta * w[i] • derivative[i] #: matrix multiplication with Hadamard product
                        // multiplying by the weight is actually not correct - this comment will be removed at a later point in time
                        tmp = Matrix<double>.op_DotMultiply(weightMatrix[i], derivative).Multiply(lr);
#else
                        // w[i] = w[i] - eta • derivative[i] #: Hadamard product
                        // this is the correct calculation - the prediction results are impresive
                        tmp = derivative.Multiply(lr);
#endif
                        // w[i] = w[i] - eta • derivative[i] #: Hadamard product
                        weightMatrix[i] = weightMatrix[i].Subtract(tmp);
                        // b = b - (eta * derivative)
                        biasMatrix[i] = biasMatrix[i].Subtract(deltaMatrix[i].Multiply(lr));
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
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
                return (rand.NextDouble() - 0.5f) * Properties.Settings.Default.RandomWeightModifier;
            }

            #endregion private methods

            //Signal Activation Function - also see https://en.wikipedia.org/wiki/Activation_function
            #region activation functions

            // currently only the Logistic aka Sigmoid aka Soft step function has been implemented, which probably is not the best. 
            // apparantly it can only solve linear problems, which is less than ideal
            // I will consider implementing the TanH function which can solve non-linear problems

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
                    log.Error(ex.Message);
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
                    log.Error(ex.Message);
                }

                return res;
            }

#endregion activation functions
        }

        public class Metrics
        {
            internal DNN dnn = null;

            private long numberOfBestScores = 0;
            private long numberOfWorstScores = 0;
            private long numberOfDeterioratingScores = 0;
            private long numberOfImprovingScores = 0;

            private double totalAccuracy = 0f;
            private double bestAccuracy = 0f;
            private double worstAccuracy = 100f;

            private double totScore = 0.0;
            private double prevTotScore = double.MaxValue;
            private double smallestError = double.MaxValue;
            private double largestError = 0;

            public DateTime prevStepDateTime = DateTime.Now;
            public string estimateRemainingTime = "";

            public double Accuracy
            {
                get { return totalAccuracy; }
                set
                {
                    totalAccuracy = value;

                    if (bestAccuracy < totalAccuracy)
                    {
                        bestAccuracy = totalAccuracy;
                    }

                    if (worstAccuracy > totalAccuracy)
                    {
                        worstAccuracy = totalAccuracy;
                    }
                }
            }

            public double Score
            {
                get { return totScore; }
                set
                {
                    prevTotScore = totScore;
                    totScore = value;

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

                    if (totScore > 0 && (smallestError - totScore) >= Properties.Settings.Default.RelevantScoreDelta)
                    {
                        smallestError = totScore;
                        numberOfBestScores++;
                    }

                }

            }

            public Metrics(DNN dnn)
            {
                this.dnn = dnn;
            }

            public override string ToString()
            {
                return string.Format("{0,11}{1,20}\n" +
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
                            string.Format("({0})", dnn.brain.tile),
                            "TrainStep:",
                            string.Format("({0}/{1})", dnn.brain.iteration, dnn.brain.totalNumberOfIterations),
                            "LR:",
                            dnn.LearningRate,
                            "Score:",
                            totScore,
                            "Best:",
                            smallestError,
                            "Worst:",
                            largestError,
                            "Index:",
                            string.Format("({0}/{1})", dnn.brain.imageIndex + 1, urlTrainingImages.Length),
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
                            estimateRemainingTime);
            }
        }

#endregion internal interfaces and classes

#region constants
        private const int ORIGIN_X = 20;
        private const int ORIGIN_Y = 20;
        private const int IMG_OFFSET_X = 10;
        private const int IMG_OFFSET_Y = 10;

        private static int LAYERS = Properties.Settings.Default.Layers;
        private static int TILE_SIZE = Properties.Settings.Default.ImageTileSize;
        private static readonly int NEURONS = TILE_SIZE * TILE_SIZE;

        internal readonly long totalNumberOfIterations = Properties.Settings.Default.NumberOfIterations; internal int imageIndex = 0;

#endregion constants

#region members

        private TestControl parent = null;
        public object brainLock = new object();
        private int width = 0;

        public DNN cluster = null;
        private Neuron[,] brainCells = new Neuron[LAYERS, NEURONS];

        public static string[] urlTrainingImages = null;
        public static string[] urlExpectedImages = null;

        private Bitmap trainIn = new Bitmap(TILE_SIZE, TILE_SIZE);
        private Bitmap trainOut = new Bitmap(TILE_SIZE, TILE_SIZE);
        private Bitmap trainExp = new Bitmap(TILE_SIZE, TILE_SIZE);

        private Bitmap saveIn = new Bitmap(TILE_SIZE, TILE_SIZE);
        private Bitmap saveOut = new Bitmap(TILE_SIZE, TILE_SIZE);
        private Bitmap saveExp = new Bitmap(TILE_SIZE, TILE_SIZE);

        private Thread BrainProcessorThread = null;
        private bool isActive = false;
        private bool terminate = false;
        private bool isSaving = false;
        private System.Random rand = null;

        internal long iteration = 0;
        internal int tile = 0;
        internal string comment = "Setup...";

        private Dictionary<string, Matrix<double>> imageTileMatricies = new Dictionary<string, Matrix<double>>();

#endregion members

#region properties

        public Neuron[,] BrainCells { get { return brainCells; } }

        [JsonIgnore]
        public bool Active { get; } = false;

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

        public Brain(TestControl parent, int seed, int width)
        {
            this.parent = parent;
            this.width = width;
            this.cluster = new DNN(this);

            string[] cmdArgs = Environment.GetCommandLineArgs();

            if (cmdArgs.Length < 2)
            {
                rand = new Random(seed);
                cluster.Init(seed);
            }
            else
            {
                cluster.Load(cmdArgs[1]);
            }

            Active = true;

            string urlOut = string.Format("{0}\\result", Properties.Settings.Default.UrlTrainingImages);
            if (!Directory.Exists(urlOut))
            {
                Directory.CreateDirectory(urlOut);
            }
        }

#endregion constructor

#region public methods

        public void BeforeTerminate()
        {
            terminate = true;
            comment = "Saving hyper parameters, please wait...";
        }

        public void Terminate()
        {
            SaveSnapshot();
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
        }

        public void Activate()
        {

            if (Properties.Settings.Default.Train)
            {
                PerformTraining();
            }
            return;
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

            ConnectInputInterface(false);
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
                log.Error(ex.Message);
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
                log.Error(ex.Message);
            }

        }

#warning TODO: model the object oriented brain based on the matrix brain
        public void CreateGrayMatter()
        {
            throw new NotImplementedException();
            //int x_offset = (int)width / (int)(LAYERS * 1.2);
            //int y_offset = TILE_SIZE;
            //int x = ORIGIN_X;
            //int y = ORIGIN_Y;

            //for (int i = 0; i < LAYERS; ++i)
            //{
            //    y = ORIGIN_Y;
            //    for (int j = 0; j < NEURONS; ++j)
            //    {
            //        BrainCells[i, j] = new Neuron(i, j, new Point(x, y), 42);
            //        if (i == 0)
            //            BrainCells[i, j].Threashhold = 0.0; // input neurons have no threashold, for now
            //        else if (i == LAYERS - 1) // output neurons all have the same threashold
            //            BrainCells[i, j].Threashhold = Properties.Settings.Default.DefaultThreashold;
            //        else
            //            BrainCells[i, j].Threashhold = rand.NextDouble() * (double)rand.Next(10);
            //        y += y_offset;
            //    }
            //    x += x_offset;
            //}
        }

#endregion public methods

        private void PerformTraining()
        {
            if (isActive) return;

            isActive = true;

            if (Properties.Settings.Default.UseMatrixBrain)
            {
                BrainProcessorThread = new Thread(new ThreadStart(this.TrainMatrixBrain));
            }
            BrainProcessorThread.Start();
        }

        private void TrainMatrixBrain()
        {
            comment = "training...";

            // set initial scores and best weights
            //cluster.metrics.Score = double.MaxValue;
            cluster.metrics.Score = 0d;
            cluster.metrics.Accuracy = 0d;
            iteration = 0;

            while (iteration < totalNumberOfIterations && !terminate)
            {
                double accuracy = 0d;
                cluster.metrics.Score = ProcessImagesViaMatrix(ref accuracy);

                cluster.metrics.Accuracy = accuracy;

                if (cluster.metrics.Score <= Properties.Settings.Default.RelevantScoreDelta)
                {
                    // we found the perfect net for the model
                    break;
                }

                parent.AddDataPoint(cluster.metrics.Score, cluster.metrics.Accuracy, (ulong)iteration + 1);

                if (iteration % Properties.Settings.Default.Snapshot == 0
                    && !terminate)
                {
                    SaveSnapshot();
                }

                iteration++;
                if ((iteration % Properties.Settings.Default.ChangeAfterIterations) == 0)
                {
                    cluster.LearningRate = cluster.LearningRate * Properties.Settings.Default.DeltaChangeBase;
                }

                double stepDuration = (DateTime.Now - cluster.metrics.prevStepDateTime).TotalSeconds;
                cluster.metrics.prevStepDateTime = DateTime.Now;

                double estTotDuration = (totalNumberOfIterations - iteration) * stepDuration;
                TimeSpan estTotTimeSpan = TimeSpan.FromSeconds(estTotDuration);
                cluster.metrics.estimateRemainingTime = String.Format("{0:d\\.hh\\:mm\\:ss}", estTotTimeSpan); // shorter format
            }

            SaveSnapshot();
        }

        public static void MatrixToImage(Matrix<double> m, ref Bitmap image)
        {
            IEnumerable<Tuple<int, int, double>> e = m.EnumerateIndexed();
            foreach (Tuple<int, int, double> t in e)
            {
                int x = t.Item1 / TILE_SIZE;
                int y = t.Item1 % TILE_SIZE;

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

        public static Matrix<double> ImageToMatrix(Bitmap image)
        {
            Matrix<double> matrix = Matrix<double>.Build.Dense(NEURONS, 1);
            for (int x = 0; x < TILE_SIZE; x++)
            {
                for (int y = 0; y < TILE_SIZE; y++)
                {
                    Color px = image.GetPixel(x, y);
                    matrix[(x * TILE_SIZE) + y, 0] = (double)(px.B + px.G + px.R) / (3.0 * 255.0);
                }
            }
            return matrix;
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

                int columns = image.Width / TILE_SIZE;
                int rows = image.Height / TILE_SIZE;

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
                                    BrainGUI.Apply(randR, randC, image, expImage);
                                }
                            }
                        }

                        string key = string.Format("{0} {1}", randR, randC);
                        Matrix<double> inputMatrix = null;
                        Matrix<double> expectedMatrix = null;
                        if (!imageTileMatricies.ContainsKey(key))
                        {
                            using (Graphics g = Graphics.FromImage(trainIn))
                            {
                                g.DrawImage(image, new Rectangle(0, 0, Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize), new Rectangle(randR * Properties.Settings.Default.ImageTileSize, randC * Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize), GraphicsUnit.Pixel);
                            }
                            using (Graphics g = Graphics.FromImage(trainExp))
                            {
                                g.DrawImage(expImage, new Rectangle(0, 0, Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize), new Rectangle(randR * Properties.Settings.Default.ImageTileSize, randC * Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize, Properties.Settings.Default.ImageTileSize), GraphicsUnit.Pixel);
                            }

                            inputMatrix = ImageToMatrix(trainIn); // this is the original gray image
                            expectedMatrix = ImageToMatrix(trainExp);
                            imageTileMatricies[key] = inputMatrix;
                            imageTileMatricies[string.Format("x {0}", key)] = expectedMatrix;
                        }
                        else
                        {
                            inputMatrix = imageTileMatricies[key];
                            expectedMatrix = imageTileMatricies[string.Format("x {0}", key)];

                        }
                        double tileAcc = 0.0;
                        err = cluster.Train(inputMatrix, expectedMatrix, ref tileAcc);
                        acc += tileAcc;
                        totError += err;
                        totNumOfTiles++;
                        accuracy = acc / (double)totNumOfTiles;

                        // this is totally not clear, but the accuracy here is actually totalAccuracy which is globaly used and has an effect on best and worst accuracy
                        // need to refactor to clean this. Probably should use Properties here to make things cleaner
                        cluster.metrics.Accuracy = accuracy;

                        if (Properties.Settings.Default.EnablePreview)
                        {
                            lock (brainLock)
                            {
                                Matrix<double> res = cluster.Predict(inputMatrix);
                                BrainGUI.ApplyOutputMatrix(res);
                            }
                        }
                        tile++;
                        if (terminate) break;
                    }
                }

                if (image != null) image.Dispose();
                if (expImage != null) expImage.Dispose();

                inputIndex++;
            }
            imageIndex = 0;

            // set return values accuracy and error
            return totError;
        }

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
                    int columns = input.Width / TILE_SIZE;
                    int rows = input.Height / TILE_SIZE;

                    Bitmap prediction = new Bitmap(columns * TILE_SIZE, rows * TILE_SIZE);

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
                                    g.DrawImage(input, new Rectangle(0, 0, TILE_SIZE, TILE_SIZE), new Rectangle(r * TILE_SIZE, c * TILE_SIZE, TILE_SIZE, TILE_SIZE), GraphicsUnit.Pixel);
                                }
                                using (Graphics g = Graphics.FromImage(saveExp))
                                {
                                    g.DrawImage(expected, new Rectangle(0, 0, TILE_SIZE, TILE_SIZE), new Rectangle(r * TILE_SIZE, c * TILE_SIZE, TILE_SIZE, TILE_SIZE), GraphicsUnit.Pixel);
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
                                g.DrawImage(saveOut, new Rectangle(r * TILE_SIZE, c * TILE_SIZE, TILE_SIZE, TILE_SIZE), new Rectangle(0, 0, TILE_SIZE, TILE_SIZE), GraphicsUnit.Pixel);
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
                        g.DrawString(string.Format("A:{0:000.000000} S:{1:0.0000}", totAcc, cluster.metrics.Score), BrainGUI.fontSnaptshot, (totAcc / 100.0 < Properties.Settings.Default.PositiveResult) ? BrainGUI.brushRed : BrainGUI.brushGreen, new Point(10, TILE_SIZE));
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
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            isSaving = false;
        }

        private void SaveImageTiles(string timeToken, double score, double accurary, Bitmap input, Bitmap output, Bitmap expected, int index)
        {
            string urlOutput = string.Format("{0}\\result\\{1}\\{2:000000}.png", Properties.Settings.Default.UrlTrainingImages, timeToken, index);

            Bitmap result = new Bitmap(TILE_SIZE + TILE_SIZE + TILE_SIZE + 8, TILE_SIZE + 20);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(input, new Point(0, 0));
                g.DrawImage(output, new Point(TILE_SIZE + 2, 0));
                g.DrawImage(expected, new Point((TILE_SIZE + TILE_SIZE + 4), 0));
                g.DrawString(string.Format("A:{0:000.000000} S:{1:0.0000}", accurary, score), SystemFonts.DefaultFont, BrainGUI.brushGreen, new Point(0, TILE_SIZE));
            }

            result.Save(urlOutput, System.Drawing.Imaging.ImageFormat.Png);
            result.Dispose();
        }

        private void ResetNeurons()
        {
            foreach (Neuron neuron in BrainCells)
            {
                neuron.Reset();
            }
        }

        private void CleanupNeurons()
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

        private void ConnectInputInterface(bool force)
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
                    BrainCells[0, i].CreateSynapseConnection(ESynapsisType.Activator, weight);
                }
            }
        }

        private void MouldImageBrain()
        {
            //// for each layer
            //for (int i = 0; i < LAYERS - 1; ++i)
            //{

            //    // create random connections to next layer
            //    for (int j = 0; j < NEURONS; ++j)
            //    {
            //        Neuron sender = BrainCells[i, j];

            //        // point-to-point
            //        Neuron receiver = BrainCells[i + 1, j];
            //        Synapse syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, FAC);
            //        sender.AddAxonConnection(syn);

            //        // point-to-nearest-neighbor
            //        foreach (int h in NEIGHBOURS)
            //        {
            //            int pos = j + h;
            //            if (pos >= 0 && pos < NEURONS)
            //            {
            //                receiver = BrainCells[i + 1, pos];
            //                syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, NEIGHBOURS_FAC);
            //                sender.AddAxonConnection(syn);
            //            }
            //        }

            //        // point-to-furthest-neighbour
            //        foreach (int q in FAR_NEIGHBOURS)
            //        {
            //            int pos = j + q;
            //            if (pos >= 0 && pos < NEURONS)
            //            {
            //                receiver = BrainCells[i + 1, pos];
            //                syn = receiver.CreateSynapseConnection(ESynapsisType.Activator, FAR_NEIGHBOURS_FAC);
            //                sender.AddAxonConnection(syn);
            //            }
            //        }
            //    }
            //}
        }

        private void MouldBrain()
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

#region IShutdown Members

        public void Shutdown()
        {
            if (!Properties.Settings.Default.UseMatrixBrain)
            {
                for (int i = 0; i < LAYERS; ++i)
                {
                    for (int j = 0; j < NEURONS; ++j)
                    {
                        BrainCells[i, j].Shutdown();
                    }
                }
            }
        }

#endregion
    }
}
