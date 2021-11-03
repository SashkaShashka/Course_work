using System;
using System.Collections.Generic;

namespace course_work_4
{
    public class Solver
    {
        protected double R;
        protected double T;
        protected double c;
        protected double k;
        protected double alpha;
        protected double u0;
        protected int I;
        protected int K;

        protected double gama;

        protected double ht;
        protected double hr;

        List<Layer> horLayers;
        List<Layer> vertLayers;

        protected double[] layer_1;
        protected double[] layer_2;

        protected double[] a;
        protected double[] b;

        public virtual void Init(double c, double k, double alpha, double u0, double R, double T, int I, int K)
        {
            this.R = R;
            this.T = T;
            this.c = c;
            this.k = k;
            this.alpha = alpha;
            this.u0 = u0;
            this.I = I;
            this.K = K;

            hr = R / I;
            ht = T / K;

            gama = k * ht / 2 / c / hr / hr;

            layer_1 = new double[I + 1];
            layer_2 = new double[I + 1];

            a = new double[I];
            b = new double[I];

            setFirstLayer();

            // generate alpha
            a[0] = 6 * gama / (1 + 6 * gama);
            for (int i = 1; i < I; i++)
                a[i] = -C(i) / (A(i) * a[i - 1] + (1 + 2 * gama));
        }
        public void SetCollectors(List<Layer> horLayers, List<Layer> vertLayers)
        {
            this.horLayers = horLayers;
            this.vertLayers = vertLayers;
        }

        public List<Layer> getHorizontalLayers()
        {
            return horLayers;
        }
        public List<Layer> getVerticalLayers()
        {
            return vertLayers;
        }

        public event Action Finish;
        public event Action<double> Process;

        public void Start()
        {
            int index = 0;
            int numHorColl = 0;

            while(index <= K)
            {
                CollectVertData(index);
                Step();
                if(numHorColl < horLayers.Count && index == horLayers[numHorColl].Index)
                    layer_1 = horLayers[numHorColl++].swapArray(layer_1);
                swapLayers();
                Process?.Invoke(index * 1.0 / K);
                index++;
            }
            Finish?.Invoke();
        }
        protected void Step()
        {
            // generate beta
            b[0] = ((1 - 6 * gama) * layer_1[0] + 6 * gama * layer_1[1]) / (1 + 6 * gama);
            for (int i = 1; i < I; i++)
            {
                double Fi = -A(i) * layer_1[i - 1] + (1 - 2 * gama) * layer_1[i] - C(i) * layer_1[i + 1];
                b[i] = (Fi - A(i) * b[i - 1]) / (A(i) * a[i - 1] + (1 + 2 * gama));
            }

            // find layer
            layer_2[I] = (layer_1[I - 1] + (1 / gama / 2 - alpha * (1 + hr/R) * hr - 1) * layer_1[I] + 
                2 * alpha * (1 + hr / R) * hr * u0 + b[I - 1]) / (-a[I-1] + (1 + 1/gama/2 + alpha * (1 + hr / R) * hr));
            for (int i = I - 1; i >= 0; i--)
                layer_2[i] = a[i] * layer_2[i + 1] + b[i];
        }
        protected void CollectVertData(int index)
        {
            foreach(var x in vertLayers)
                x.Array[index] = layer_1[x.Index];
        }
        protected void swapLayers()
        {
            double[] t = layer_1;
            layer_1 = layer_2;
            layer_2 = t;
        }
        protected void setFirstLayer()
        {
            for (int i = 0; i <= I; i++) {
                double x = Math.PI * hr * i / R;
                layer_1[i] = 12*(1 + Math.Cos(x));
            }
        }
        double A(int i)
        {
            return -gama * (1 - 1.0 / i);
        }
        double C(int i)
        {
            return -gama * (1 + 1.0 / i);
        }
    }
}
