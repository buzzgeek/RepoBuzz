using System;
using System.Threading;
using System.Drawing;
using Newtonsoft.Json;

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
	public enum ESynapsisType
	{
		Activator,
		Inhibitor
	}

    /// <summary>
    /// Summary description for Synapse.
    /// </summary>
    /// 

    public class SynapseGUI
    {
        #region public static members

        public static readonly Pen penActivate = new Pen(Color.Green);
        public static readonly Pen penInhibit = new Pen(Color.Red);
        public static readonly Pen penDisabled = new Pen(Color.Gray);
        
        #endregion public static members

        #region public static methods 

        public static void Draw(Graphics g, Synapse s)
        {
            lock (s)
            {
                Pen pen = s.Disable ? penDisabled : (s.Type == ESynapsisType.Activator ? penActivate : penInhibit);

                if (s.Sender != null && s.Receiver != null)
                {
                    Point begin = s.Sender.Position;
                    Point end = s.Receiver.Position;
                    begin.X += s.Sender.Radius;
                    end.X -= s.Receiver.Radius;
                    g.DrawLine(pen, begin, end);
                }
            }
        }

        #endregion public static methods
    }

    public class Synapse : IShutdown
	{
		#region members

        private const double TOLERANCE = 0.00001;
		private string id = "";
		private bool signal = false; // indicated if there is a signal in the Synapse
        private ESynapsisType type = ESynapsisType.Activator; // inhibitor or activator
        private AutoResetEvent signalGet = null;
        private AutoResetEvent signalSet = null;
		private Neuron sender = null;
		private Neuron receiver = null;
		private bool disable = false;
		private double weight = 1.0; // the Synapse' strength;
        private double score = 0.0;
        private double bestScore = double.MinValue;
        private double bestWeight = 1.0;
        private int layerReceiver = 0;
        private int indexReceiver = 0;
        private int layerSender = 0;
        private int indexSender = 0;
        private Random rand = null;
		private bool terminate = false;

		#endregion members

		#region properties

        public int LayerReceiver
        {
            get { return layerReceiver; }
            set { layerReceiver = value; }
        }

        public int IndexReceiver
        {
            get { return indexReceiver; }
            set { indexReceiver = value; }
        }

        public int LayerSender
        {
            get { return layerSender; }
            set { layerSender = value; }
        }

        public int IndexSender
        {
            get { return indexSender; }
            set { indexSender = value; }
        }

		public bool Disable
		{
			set { disable = value; }
            get { return disable; }
		}

		public string ID
		{
			get
			{ 
				if( id.Length == 0)
				{
					id = Guid.NewGuid().ToString();
				}
				return id;
			}
            set
            {
                id = value;
            }
		}

		public double Weight
		{
			get { return weight; }
			set { weight = value; }
		}

        public double Score
        {
            get { return score; }
            set
            {
                score = value;
                if (score > bestScore)
                {
                    // if the score is better track the new weight (e.g. greater)
                    bestScore = score;
                    bestWeight = weight;
                }
                else if (Math.Abs(score - bestScore) < TOLERANCE &&
                  Math.Abs(weight) < Math.Abs(bestWeight))
                {
                    // if the scores are equal track the weight if its absolute is smaller than before
                    bestWeight = weight;
                }
                //if (Math.Abs(bestWeight) < TOLERANCE) bestWeight = 0.0;
            }
        }

		public ESynapsisType Type
		{
			get { return type; }
			set { type = value; }
		}

        [JsonIgnore]
		public bool Signal
		{
			get 
            {
                while (!disable && !signalSet.WaitOne(100));                
                bool tmp = false;
                lock (this)
                {
                    tmp = signal;
                }
                signalGet.Set();

                return tmp;
            }

			set	
            {
                while (!disable && !signalGet.WaitOne(100)) ;                

                lock (this)
                {
                    signal = value;
                }
                signalSet.Set();
            }
		}

        [JsonIgnore]
		public Neuron Sender
		{
			get{ return sender; }
			set
            { 
                sender = value;
                layerSender = sender.Layer;
                indexSender = sender.Index;
            }
		}

        [JsonIgnore]
		public Neuron Receiver
		{
			get{ return receiver; }
			set
            { 
                receiver = value;
                layerReceiver = receiver.Layer;
                indexReceiver = receiver.Index;
            }
		}

		#endregion properties

		#region constructor
		/// <summary>
		/// Constructor
		/// </summary>
		public Synapse(Neuron neuroSender, Neuron neuroReceiver, int seed, ESynapsisType type, double weight)
		{
			rand = new Random(seed);
			sender = neuroSender;
			receiver = neuroReceiver;
            signalGet = new AutoResetEvent(true);
            signalSet = new AutoResetEvent(false);
            this.type = type;
            this.weight = weight;
		}
		#endregion constructor

		#region destructor
		~Synapse()
		{
			disable = true;
		}
		#endregion destructor

		#region public methods

        public void UseOptimum()
        {
            weight = bestWeight;
            score = bestScore;
        }

		public void Reset()
		{
            signalGet.Set();
		}

		public void Rethink()
		{
            lock(this)
            {
				weight = rand.NextDouble();
				type = rand.Next(2) > 0 ? ESynapsisType.Activator : ESynapsisType.Inhibitor;
            }
		}

		#endregion pubic methods

		#region IShutdown Members

        public void Shutdown()
        {
            if (!terminate)
            {
                disable = true;
                terminate = true;
                signalGet.Set();
                signalSet.Set();
            }
        }

		#endregion
	}
}
