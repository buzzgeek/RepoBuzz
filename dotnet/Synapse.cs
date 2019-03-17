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
	public class Synapse : IShutdown
	{
		#region members
        private const double TOLERANCE = 0.00001;

		// global unique identifier of this synapis
		private string id = "";

		
		// indicated if there is a signal in the Synapse
		private bool signal = false;

        private AutoResetEvent signalGet = null;
        private AutoResetEvent signalSet = null;

		private Pen penActivate = new Pen (Color.Green);
		private Pen penInhibit = new Pen (Color.Red);
	
		private Pen penSet = new Pen (Color.Gold);
		private Pen penGet = new Pen (Color.Blue);
		private Pen penDisabled = new Pen (Color.Gray);

		private Neuron sender = null;
		private Neuron receiver = null;
		private bool disable = false;

		// the Synapse' strength;
		private double weight = 1.0;
        private double score = 0.0;
        private double bestScore = double.MinValue;
        private double bestWeight = 1.0;

        private int layerReceiver = 0;
        private int indexReceiver = 0;
        private int layerSender = 0;
        private int indexSender = 0;

        
		// inhibitor or activator
		private ESynapsisType type = ESynapsisType.Activator;

		private Random rand = null;

		private bool disposed = false;

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

		
		public void Draw (Graphics g)
		{
            lock (this)
            {
                Pen pen = (type == ESynapsisType.Activator ? penActivate : penInhibit);

                //Point end = neuron != null ? neuron.Position : new Point(begin.X + 40, begin.Y); 
                if (sender != null && receiver != null)
                {
                    Point begin = sender.Position;
                    Point end = receiver.Position;
                    begin.X += sender.Radius;
                    end.X -= receiver.Radius;
                    g.DrawLine(pen, begin, end);
                }
            }
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

		#region protected methods
		#endregion protecetd methods

		#region private methods
		#endregion private methods

		#region IDisposable Members

        public void Shutdown()
        {
            if (!disposed)
            {
                disable = true;
                disposed = true;
                signalGet.Set();
                signalSet.Set();
            }
        }

		#endregion
	}
}
