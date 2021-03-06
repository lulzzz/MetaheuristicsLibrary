﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaheuristicsLibrary.Misc;

namespace MetaheuristicsLibrary.SolversSO
{
    /// <summary>
    /// Interface for Single objective solver.
    /// </summary>
    public abstract class SO_Solver
    {
        /// <summary>
        /// Decision variable count
        /// </summary>
        public int n { get; private set; }
        /// <summary>
        /// Lower bound for each variable.
        /// </summary>
        public double[] lb { get; private set; }
        /// <summary>
        /// Upper bound for each variable.
        /// </summary>
        public double[] ub { get; private set; }
        /// <summary>
        /// Indicate, which variable is integer
        /// </summary>
        public bool[] xint { get; private set; }
        /// <summary>
        /// Maximum function evaluations.
        /// </summary>
        public int evalmax { get; private set; }
        /// <summary>
        /// current count of function evaluation calls
        /// </summary>
        public int evalcount { get; protected set; }
        /// <summary>
        /// Evaluation function.
        /// </summary>
        public Func<double[], double> evalfnc { get; private set; }
        /// <summary>
        /// Variable vector of final solution.
        /// </summary>
        public double[] xopt { get; protected set; }
        /// <summary>
        /// Cost of final solution.
        /// </summary>
        public double fxopt { get; protected set; }

        protected RandomDistributions rnd;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="ub"></param>
        /// <param name="xinteger">indicate, which variable is integer</param>
        /// <param name="evalmax"></param>
        /// <param name="evalfnc"></param>
        /// <param name="seed"></param>
        public SO_Solver(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed)
        {
            this.lb = lb;
            this.ub = ub;
            this.xint = xint;
            this.evalcount = 0;
            this.evalmax = evalmax;
            this.evalfnc = evalfnc;
            this.n = lb.Length;
            this.rnd = new RandomDistributions(seed);

            this.fxopt = double.MaxValue;
            this.xopt = new double[this.n];
        }



        /// <summary>
        /// Solve. Override this.
        /// </summary>
        public abstract void solve();

        /// <summary>
        /// Storing current best solution.
        /// Could be, that the algorithm doesn't have memory.
        /// </summary>
        protected abstract void storeCurrentBest();

        /// <summary>
        /// stopping criterion for FrOG
        /// </summary>
        /// <returns></returns>
        protected abstract bool CheckIfNaN(double fxtest);


        /// <summary>
        /// Get the variable vector of the final solution. Call this after solving complete.
        /// </summary>
        /// <returns>Variable vector.</returns>
        public double[] get_Xoptimum()
        {
            return this.xopt;
        }

        /// <summary>
        /// Get the cost value of the final solution. Call this after solving is complete.
        /// </summary>
        /// <returns>Cost value.</returns>
        public double get_fxoptimum()
        {
            return this.fxopt;
        }

        /// <summary>
        /// checking bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="_lb"></param>
        /// <param name="_ub"></param>
        protected void checkBounds(ref double[] x, double[] _lb, double[] _ub)
        {
            for (int i = 0; i < this.n; i++)
            {
                if (x[i] < _lb[i]) x[i] = _lb[i];
                else if (x[i] > _ub[i]) x[i] = _ub[i];
            }
        }



    }


    /// <summary>
    /// Stochastic Hill-Climbing,
    /// <para/>Pseudocode from: Clever Algorithms: Nature-Inspired Programming Recipes (Jason Brownlee)
    /// </summary>
    public class Hillclimber : SO_Solver
    {
        //source: Stochastic Hill-Climbing, in: Clever Algorithms: Nature-Inspired Programming Recipes (Jason Brownlee)
        //
        //Input: Itermax, ProblemSize 
        //Output: Current 
        //Current  <- RandomSolution(ProblemSize)
        //For (iteri ∈ Itermax )
        //    Candidate  <- RandomNeighbor(Current)
        //    If (Cost(Candidate) >= Cost(Current))
        //        Current  <- Candidate
        //    End
        //End
        //Return (Current)


        private double[] xtest;
        private double[] x;
        private double fxtest;
        private double fx;
        private double[] x0;


        /// <summary>
        /// Stepsize.
        /// </summary>
        public double stepsize { get; private set; }


        /// <summary>
        /// Initialize a stochastic hill climber optimization algorithm. Assuming minimization problems.
        /// </summary>
        /// <param name="lb">Lower bound for each variable.</param>
        /// <param name="ub">Upper bound for each variable.</param>
        /// <param name="stepsize">Stepsize.</param>
        /// <param name="evalmax">Maximum iterations.</param>
        /// <param name="evalfnc">Evaluation function.</param>
        /// <param name="seed">Seed for random number generator.</param>
        public Hillclimber(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, double stepsize, double[] x0 = null) :
            base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.stepsize = stepsize;


            this.x0 = x0 ?? new double[0];
        }

        /// <summary>
        /// Minimizes an evaluation function using stochastic hill climbing.
        /// </summary>
        public override void solve()
        {
            int n = lb.Length;
            this.x = new double[n];

            double[] stdev = new double[n];

            if (this.x0.Length == base.n)
            {
                this.x0.CopyTo(this.x, 0);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    this.x[i] = rnd.NextDouble() * (ub[i] - lb[i]) + lb[i];
                    stdev[i] = stepsize * (ub[i] - lb[i]);
                }
            }
            this.fx = evalfnc(this.x);

            for (base.evalcount = 0; base.evalcount < evalmax; base.evalcount++)
            {
                this.xtest = new double[n];
                for (int i = 0; i < n; i++)
                {
                    this.xtest[i] = rnd.NextGaussian(this.x[i], stdev[i]);
                    if (this.xtest[i] > ub[i]) this.xtest[i] = ub[i];
                    else if (this.xtest[i] < lb[i]) this.xtest[i] = lb[i];
                }
                this.fxtest = evalfnc(this.xtest);

                if (CheckIfNaN(this.fxtest)) return;


                storeCurrentBest();
            }



        }


        protected override void storeCurrentBest()
        {
            if (this.fxtest < this.fx)
            {
                this.xtest.CopyTo(this.x, 0);
                this.fx = this.fxtest;

                base.xopt = new double[n];
                this.x.CopyTo(base.xopt, 0);
                base.fxopt = this.fx;
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            if (Double.IsNaN(fxtest))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// Particle Swarm Optimizer (PSO)
    /// Fully informed PSO, which uses multiple neighbouring particle vectors for updating velocity.
    /// The canonical PSO only considers 2 vectors for updating: a particle's own best and the global / local best particle.
    /// Source:     Mendes R., Kennedy J., Neves J. (2004). Fully Informed Particle Swarm: Simpler, Maybe Better.
    /// 2nd Source: Poli R., Kennedy J., Blackwell T. (2007). Particle swarm optimization - An overview.
    /// </summary>
    public class PSO : SO_Solver
    {
        //Poli wt al. (2007), eqt. 5
        //
        //v_i <- χ (v_i + 1/K (Σ_n∈K (U(0,ϕ) ⨂ (x_n,i,best - x_i) ) ) )
        //x_i <- x_i + v_i
        //
        // with: 
        // x_n,i,best   = best foud so far of neighbour n of particle i
        // K            = number of neighbours
        // χ            = constriction coefficient, typically 0.7298
        // ϕ            = 4.1?




        /// <summary>
        /// cost values of new population
        /// </summary>
        public double[] fx_pop { get; private set; }
        /// <summary>
        /// decision variables of new population
        /// </summary>
        public double[][] x_pop { get; private set; }

        private double[][] x0;
        private double[] fx0;

        /// <summary>
        /// population size
        /// </summary>
        private int popsize;
        /// <summary>
        /// constriction coefficient. typically 0.7298
        /// </summary>
        private double chi;
        /// <summary>
        /// upper bound for random number for attraction of particle i to best particle j. 4.1?
        /// </summary>
        private double phi;
        /// <summary>
        /// constriction coefficient for random initial velocity. fraction of the search domain.
        /// </summary>
        private double v0max;
        /// <summary>
        /// velocities
        /// </summary>
        private double[][] v;

        /// <summary>
        /// best solution vector found so far, per particle
        /// </summary>
        private double[][] px_best;
        /// <summary>
        /// best cost value found so far, per particle
        /// </summary>
        private double[] pfx_best;
        /// <summary>
        /// Per particle, 4 indices of neighbours. excluding own best. cp. Mendes et al. (2004).
        /// </summary>
        private int[][] indK;

        /// <summary>
        /// 0 = uniform, 1= gaussian
        /// </summary>
        private int x0samplingmode;

        /// <summary>
        /// updating pxbest mode. mode 0 = update after looping trough entire population. mode 1 = update after each function evaluation.
        /// </summary>
        private int pxupdatemode;


        /// <summary>
        /// initial stepsize, in case of gaussian initial sampling
        /// </summary>
        private double[] s0;

        /// <summary>
        /// phi1 = own best, phi2 = global best
        /// </summary>
        private double phi1, phi2;


        /// <summary>
        /// false means inertia weight PSO, true is fipso
        /// </summary>
        private int psomode;

        /// <summary>
        /// probability for uniform random mutation of integer variable
        /// </summary>
        private double intmut;

        private double mutstdev;



        /// <summary>
        /// using von Neumann topology.
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="ub"></param>
        /// <param name="xint"></param>
        /// <param name="evalmax"></param>
        /// <param name="evalfnc"></param>
        /// <param name="seed"></param>
        /// <param name="settings"></param>
        /// <param name="x0"></param>
        public PSO(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[][] x0 = null)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.x0 = x0 ?? new double[0][];

            if (settings.ContainsKey("psomode"))
            {
                this.psomode = Convert.ToInt16(settings["psomode"]);
            }
            else
            {
                this.psomode = 0;       //fipso... 1=pso inertia, 2 = pso constriction coeff
            }

            if (settings.ContainsKey("phi1"))
            {
                this.phi1 = Convert.ToDouble(settings["phi1"]);
            }
            else
            {
                this.phi1 = 2.05;
            }

            if (settings.ContainsKey("phi2"))
            {
                this.phi2 = Convert.ToDouble(settings["phi2"]);
            }
            else
            {
                this.phi2 = 2.05;
            }


            if (settings.ContainsKey("popsize"))
            {
                this.popsize = Convert.ToInt32(settings["popsize"]);
            }
            else
            {
                this.popsize = 24;      // population size.
            }

            if (settings.ContainsKey("chi"))
            {
                this.chi = Convert.ToDouble(settings["chi"]);
            }
            else
            {
                this.chi = 0.1;      // constriction coefficient
            }


            if (settings.ContainsKey("phi"))
            {
                this.phi = Convert.ToDouble(settings["phi"]);
            }
            else
            {
                this.phi = 4;         // attraction to best particle 
            }

            if (settings.ContainsKey("v0max"))
            {
                this.v0max = Convert.ToDouble(settings["v0max"]);
            }
            else
            {
                this.v0max = 0.2;       // max velocity at initialisation. fraction of domain.
            }

            if (settings.ContainsKey("x0samplingmode"))
            {
                this.x0samplingmode = Convert.ToInt16(settings["x0samplingmode"]);
            }
            else
            {
                this.x0samplingmode = 0;// 0 = uniform, 1 = gaussian
            }

            if (settings.ContainsKey("pxupdatemode"))
            {
                this.pxupdatemode = Convert.ToInt16(settings["pxupdatemode"]);
            }
            else
            {
                this.pxupdatemode = 0;  //0 = update after population. 1 = update after each evaluation
            }

            this.s0 = new double[base.n];

            if (settings.ContainsKey("s0"))
            {
                for (int i = 0; i < base.n; i++) s0[i] = Convert.ToDouble(settings["s0"]);
            }
            else
            {
                for (int i = 0; i < base.n; i++) s0[i] = 1.0;       //initial step size in case of gaussian sampling
            }



            if (settings.ContainsKey("intmut"))
            {
                this.intmut = Convert.ToDouble(settings["intmut"]);
            }
            else
            {
                this.intmut = 0.5;
            }

            if (settings.ContainsKey("mutstdev"))
            {
                this.mutstdev = Convert.ToDouble(settings["mutstdev"]);
            }
            else
            {
                this.mutstdev = 0.3;
            }
        }

        public override void solve()
        {
            if (psomode == 0)
            {
                this.initializeNeighbourhood();
                this.initialSamples();

                int pUpdate = this.popsize;
                if (this.pxupdatemode == 1) pUpdate = 1;

                int pNow = 0;

                while (base.evalcount < base.evalmax)
                {
                    for (int p = 0; p < pUpdate; p++)
                    {
                        double[] v_new;
                        double[] x_new;
                        double fx_new;
                        this.updateParticleFIPSO(out v_new, out x_new, this.v[pNow], this.x_pop[pNow], this.px_best, this.indK[pNow]);
                        fx_new = base.evalfnc(x_new);
                        this.v[pNow] = new double[base.n];
                        this.x_pop[pNow] = new double[base.n];
                        v_new.CopyTo(this.v[pNow], 0);
                        x_new.CopyTo(this.x_pop[pNow], 0);
                        this.fx_pop[pNow] = fx_new;

                        this.updateParticlesBest(ref this.px_best[pNow], ref this.pfx_best[pNow], x_new, fx_new);

                        //Console.WriteLine("fx,x1,x2,{0},{1},{2}", fx_new, x_new[0], x_new[1]);
                        base.evalcount++;
                        pNow++;
                        if (pNow == this.popsize) pNow = 0;

                        this.storeCurrentBest();
                        if (this.CheckIfNaN(fx_new))
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                this.initialSamples();
                int pUpdate = this.popsize;
                if (this.pxupdatemode == 1) pUpdate = 1;

                int pNow = 0;

                while (base.evalcount < base.evalmax)
                {
                    for (int p = 0; p < pUpdate; p++)
                    {
                        double[] v_new;
                        double[] x_new;
                        double fx_new;
                        this.updateParticle(out v_new, out x_new, this.v[pNow], this.x_pop[pNow], this.xopt, this.px_best[pNow], this.chi, this.phi1, this.phi2, this.psomode);
                        fx_new = base.evalfnc(x_new);
                        this.v[pNow] = new double[base.n];
                        v_new.CopyTo(this.v[pNow], 0);
                        this.x_pop[pNow] = new double[base.n];
                        x_new.CopyTo(this.x_pop[pNow], 0);      //magically, its also copied to pxbest???!!!
                        this.fx_pop[pNow] = fx_new;

                        this.updateParticlesBest(ref this.px_best[pNow], ref this.pfx_best[pNow], x_new, fx_new);

                        base.evalcount++;
                        pNow++;
                        if (pNow == this.popsize) pNow = 0;

                        this.storeCurrentBest();
                        if (this.CheckIfNaN(fx_new))
                        {
                            return;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// von Neumann topology, excluding own particle
        /// </summary>
        private void initializeNeighbourhood()
        {
            double sqrtPop = Math.Sqrt(this.popsize);
            int cols = (int)Math.Floor(sqrtPop);
            int rows = (int)Math.Round((Convert.ToDouble(this.popsize) / Convert.ToDouble(cols)), 0);
            this.popsize = cols * rows;         //making sure popsize can be divided into a cols*rows matrix

            this.indK = new int[this.popsize][];
            for (int i = 0; i < this.popsize; i++)
                this.indK[i] = new int[4];

            int count = 0;
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    this.indK[count][0] = count - cols;
                    this.indK[count][1] = count - 1;
                    this.indK[count][2] = count + 1;
                    this.indK[count][3] = count + cols;

                    if ((count + 1) % cols == 0) this.indK[count][2] = count - cols + 1;
                    else if ((count + 1) % cols == 1) this.indK[count][1] = count + cols - 1;

                    if (count - cols < 0) this.indK[count][0] = count + this.popsize - cols;
                    if (count + cols > this.popsize - 1) this.indK[count][3] = count - this.popsize + cols;

                    count++;
                }
            }
        }

        /// <summary>
        /// sampling initial population x0 and fx0, initial velocity v0, and store as current best particles.
        /// </summary>
        private void initialSamples()
        {
            int existing_p = this.x0.Length;
            this.fx0 = new double[this.popsize];
            bool x0exists = false;
            if (this.x0.Length > 0)
            {
                x0exists = true;
                double[][] _xpop = new double[this.popsize][];
                for (int p = 0; p < this.x0.Length; p++)
                {
                    for (int i = 0; i < base.n; i++)
                    {
                        if (base.xint[i])
                        {
                            this.x0[p][i] = Math.Round(this.x_pop[0][i], 0);
                        }
                    }
                    _xpop[p] = new double[base.n];
                    this.x0[p].CopyTo(_xpop[p], 0);
                    this.fx0[p] = base.evalfnc(this.x0[p]);
                    //Console.WriteLine("fx,x1,x2,{0},{1},{2}", this.fx0[p], this.x0[p][0], this.x0[p][1]);
                    this.evalcount++;
                }
                this.x0 = new double[this.popsize][];   //i'm doing this, because x0.Length at initialisation could be < popsize
                _xpop.CopyTo(this.x0, 0);
            }
            else
            {
                this.x0 = new double[this.popsize][];
            }

            double[] xbasepoint = new double[base.n];
            if (x0exists)
            {
                this.x0[0].CopyTo(xbasepoint, 0);       //always take the first entry, no matter if more than 1 has been inputted
            }
            else
            {
                for (int i = 0; i < base.n; i++)
                {
                    xbasepoint[i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i]; //if no x0 exists, uniform sampling
                    if (base.xint[i])
                    {
                        xbasepoint[i] = Math.Round(xbasepoint[i], 0);
                    }
                }
            }
            for (int p = existing_p; p < this.popsize; p++)
            {
                this.x0[p] = new double[base.n];
                if (this.x0samplingmode == 0)
                {
                    // uniform sampling within the search domain
                    for (int i = 0; i < base.n; i++)
                    {
                        this.x0[p][i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i];
                        if (base.xint[i])
                        {
                            this.x0[p][i] = Math.Round(this.x0[p][i], 0);
                        }
                    }
                }
                else
                {
                    //gaussian sampling around a point
                    for (int i = 0; i < base.n; i++)
                    {
                        this.x0[p][i] = (base.rnd.NextGaussian(0, this.s0[i]) * (base.ub[i] - base.lb[i])) + xbasepoint[i];
                        //this.x0[p][i] = (base.rnd.NextGaussian(0, this.s0[i]) * (base.ub[i] - base.lb[i])) + xbasepoint[i];
                        if (base.xint[i])
                        {
                            this.x0[p][i] = Math.Round(this.x0[p][i], 0);
                        }
                    }
                }
                this.checkBounds(ref this.x0[p], base.lb, base.ub);
                this.fx0[p] = base.evalfnc(this.x0[p]);
                //Console.WriteLine("fx,x1,x2,{0},{1},{2}", this.fx0[p], this.x0[p][0], this.x0[p][1]);
                this.evalcount++;
            }

            //get the best
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx0[p] < base.fxopt)
                {
                    base.fxopt = this.fx0[p];
                    this.x0[p].CopyTo(base.xopt, 0);
                }
            }

            this.x_pop = new double[this.popsize][];
            this.fx_pop = new double[this.popsize];
            this.px_best = new double[this.popsize][];
            this.pfx_best = new double[this.popsize];
            this.x0.CopyTo(this.x_pop, 0);
            this.x0.CopyTo(this.px_best, 0);
            this.fx0.CopyTo(this.fx_pop, 0);
            this.fx0.CopyTo(this.pfx_best, 0);



            this.v = new double[this.popsize][];
            for (int p = 0; p < this.popsize; p++)
            {
                this.v[p] = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    //!!!!!!!!!!!!!!!! BUG
                    // v0 should be able to go into both directions, negative and positive!
                    // here, its only going in one direction basically, and then shifted to lb. if thats -, it can happen
                    // that we have - velocity.
                    //but it should be:
                    this.v[p][i] = base.rnd.NextGaussian(0, base.ub[i] - base.lb[i]) * this.v0max;

                    //this.v[p][i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i];
                    //this.v[p][i] *= this.v0max;
                }
            }
        }

        private void updateParticleFIPSO(out double[] v_new, out double[] x_new, double[] v_old, double[] x_old, double[][] px_best, int[] neighbours)
        {
            v_new = new double[base.n];
            x_new = new double[base.n];
            //v_i <- χ (v_i + 1/K (Σ_n∈K (U(0,ϕ) ⨂ (x_n,i,best - x_i) ) ) )
            //x_i <- x_i + v_i
            int K = neighbours.Length;
            for (int i = 0; i < base.n; i++)
            {
                double sumXi = 0;
                for (int k = 0; k < K; k++)
                {
                    sumXi += (rnd.NextDouble() * this.phi) * (px_best[neighbours[k]][i] - x_old[i]);
                }
                v_new[i] = v_old[i] + (sumXi / K);
                v_new[i] *= this.chi;
                if (Math.Round(v_new[i], 5) == 0)
                {
                    double rndgauss = base.rnd.NextGaussian(0, this.chi * (base.ub[i] - base.lb[i]));
                    v_new[i] = rndgauss;
                }

                x_new[i] = x_old[i] + v_new[i];
                if (base.xint[i])
                {
                    x_new[i] = Math.Round(x_new[i], 0);
                    if (base.rnd.NextDouble() < this.intmut)
                    {
                        double rndgauss = base.rnd.NextGaussian(0, this.mutstdev * (base.ub[i] - base.lb[i]));
                        //double rndgauss = base.rnd.NextGaussian(0, v_new[i] * (base.ub[i] - base.lb[i]));
                        x_new[i] = Convert.ToInt32(x_new[i] + rndgauss);
                    }
                }
            }

            base.checkBounds(ref x_new, base.lb, base.ub);
        }

        private void updateParticle(out double[] v_new, out double[] x_new, double[] v_old, double[] x_old,
            double[] gx_best, double[] ownx_best,
            double inertia, double phi1, double phi2,
            int psomode)
        {
            v_new = new double[base.n];
            x_new = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                if (psomode == 1)
                {
                    v_new[i] = inertia * v_old[i] +
                        (rnd.NextDouble() * phi1) * (ownx_best[i] - x_old[i]) +
                        (rnd.NextDouble() * phi2) * (gx_best[i] - x_old[i]);
                }
                else
                {
                    v_new[i] = inertia * (v_old[i] +
                        (rnd.NextDouble() * phi1) * (ownx_best[i] - x_old[i]) +
                        (rnd.NextDouble() * phi2) * (gx_best[i] - x_old[i]));
                }
                if (Math.Round(v_new[i], 5) == 0)
                {
                    double rndgauss = base.rnd.NextGaussian(0, inertia * (base.ub[i] - base.lb[i]));
                    v_new[i] = rndgauss;
                }
                x_new[i] = x_old[i] + v_new[i];
                if (base.xint[i])
                {
                    x_new[i] = Math.Round(x_new[i], 0);
                    if (base.rnd.NextDouble() < this.intmut)
                    {
                        double rndgauss = base.rnd.NextGaussian(0, this.mutstdev * (base.ub[i] - base.lb[i]));
                        //double rndgauss = base.rnd.NextGaussian(0, v_new[i] * (base.ub[i] - base.lb[i]));
                        x_new[i] = Convert.ToInt32(x_new[i] + rndgauss);
                    }
                }
            }

            base.checkBounds(ref x_new, base.lb, base.ub);
        }


        private void updateParticlesBest(ref double[] px_best, ref double pfx_best, double[] x_now, double fx_now)
        {
            if (fx_now < pfx_best)
            {
                x_now.CopyTo(px_best, 0);
                pfx_best = fx_now;
            }

        }


        protected override void storeCurrentBest()
        {
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx_pop[p] < base.fxopt)
                {
                    base.fxopt = this.fx_pop[p];
                    this.x_pop[p].CopyTo(base.xopt, 0);
                }
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {

            bool stop = false;
            if (Double.IsNaN(fxtest))
            {
                stop = true;
            }
            return stop;
        }


    }


    /// <summary>
    /// Simple Genetic Algorithm
    /// Goldberg (1989). Genetic Algorithms in search, optimization and machine learning.
    /// </summary>
    public class SimpleGA : SO_Solver
    {
        /// <summary>
        /// Population size
        /// </summary>
        public int popsize { get; private set; }
        /// <summary>
        /// Current generation
        /// </summary>
        public int gen { get; private set; }
        /// <summary>
        /// Maximum generations
        /// </summary>
        public int maxgen { get; private set; }
        /// <summary>
        /// length of bitstring
        /// </summary>
        private int[] lchrom;

        /// <summary>
        /// crossover probability
        /// </summary>
        public double pcross { get; private set; }
        /// <summary>
        /// mutation probability
        /// </summary>
        public double pmutation { get; private set; }

        /// <summary>
        /// extension of hypercube, for intermediate recombination
        /// </summary>
        public double d { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double r { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double k { get; private set; }

        /// <summary>
        /// sum of population fitness. required for roulette wheel
        /// </summary>
        private double sumfitness;



        /// <summary>
        /// cost values of new population
        /// </summary>
        public double[] fx_pop { get; private set; }
        /// <summary>
        /// decision variables of new population
        /// </summary>
        public double[][] x_pop { get; private set; }

        /// <summary>
        /// initial solutions
        /// </summary>
        private double[][] x0;
        private double[] fx0;

        /// <summary>
        /// how many elites to be maintained per generation
        /// </summary>
        private int elite;

        /// <summary>
        /// Simple GA, according to Goldberg 1989, chapter 3.
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="ub"></param>
        /// <param name="evalmax"></param>
        /// <param name="evalfnc"></param>
        /// <param name="seed"></param>
        /// <param name="settings">Dictionary. Should contain: population size ("popsize", int), crossover probability ("pcross", double), mutation probability ("pmut", double).</param>
        /// <param name="x0">Decision variables of initial population</param>
        /// <param name="fx0">Cost values of initial population</param>
        public SimpleGA(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[][] x0 = null)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.x0 = x0 ?? new double[0][];

            gen = 0;

            this.lchrom = new int[base.n];
            for (int i = 0; i < base.n; i++)
            {
                if (base.xint[i])
                {
                    bool[] xint_ = new bool[1];
                    xint_[0] = base.xint[i];
                    double[] lb_ = new double[1];
                    double[] ub_ = new double[1];
                    lb_[0] = base.lb[i];
                    ub_[0] = base.ub[i];

                    this.lchrom[i] = Misc.Misc.BinReqLength(xint_, lb_, ub_);
                }
                else
                {
                    this.lchrom[i] = 0;
                }
            }



            if (settings.ContainsKey("maxgen"))
            {
                maxgen = Convert.ToInt32(settings["maxgen"]);
            }
            else
            {
                maxgen = 100;
            }

            if (settings.ContainsKey("popsize"))
            {
                popsize = Convert.ToInt32(settings["popsize"]);
            }
            else
            {
                popsize = 20;
            }

            if (settings.ContainsKey("elite"))
            {
                elite = Convert.ToInt32(settings["elite"]);
            }
            else
            {
                elite = 1;
            }

            if (settings.ContainsKey("pcross"))
            {
                pcross = Convert.ToDouble(settings["pcross"]);
            }
            else
            {
                pcross = 0.7;
            }

            if (settings.ContainsKey("pmut"))
                pmutation = Convert.ToDouble(settings["pmut"]);
            else
                pmutation = 0.3;


            if (settings.ContainsKey("d"))
                d = Convert.ToDouble(settings["d"]);
            else
                d = 0.1;

            if (settings.ContainsKey("r"))
                r = Convert.ToDouble(settings["r"]);
            else
                r = 0.1;

            if (settings.ContainsKey("k"))
                k = Convert.ToDouble(settings["k"]);
            else
                k = 16;
        }

        /// <summary>
        /// Minimizes an evaluation function using a simple Genetic Algorithm.
        /// </summary>
        public override void solve()
        {
            //initialize
            initialize();

            this.x_pop = new double[this.popsize][];
            this.fx_pop = new double[this.popsize];
            this.x0.CopyTo(this.x_pop, 0);
            this.fx0.CopyTo(this.fx_pop, 0);
            //main loop
            do
            {
                Array.Sort(this.fx_pop, this.x_pop);
                double[] fitness_pop = CostToFitness(this.fx_pop);
                this.sumfitness = 0;
                for (int p = 0; p < this.popsize; p++)
                {
                    this.sumfitness += fitness_pop[p];
                }


                int nOffspring = 0;
                double[][] x_new = new double[this.popsize][];
                double[] fx_new = new double[this.popsize];
                for (int e = 0; e < this.elite; e++)
                {
                    x_new[nOffspring] = new double[base.n];
                    this.x_pop[e].CopyTo(x_new[nOffspring], 0);
                    fx_new[nOffspring] = this.fx_pop[e];
                    nOffspring++;
                }
                do
                {
                    double[] child1;
                    double[] child2;
                    double[] parent1 = new double[base.n];
                    double[] parent2 = new double[base.n];
                    parent1 = this.x_pop[select(this.sumfitness, fitness_pop)];
                    parent2 = this.x_pop[select(this.sumfitness, fitness_pop)];
                    crossovermutate(out child1, out child2, parent1, parent2, d, r, k);



                    x_new[nOffspring] = child1;
                    fx_new[nOffspring] = evalfnc(child1);
                    //Console.WriteLine("fx,x1,x2,{0},{1},{2}", fx_new[nOffspring], child1[0], child1[1]);
                    base.evalcount++;
                    if (CheckIfNaN(fx_new[nOffspring]))
                    {
                        return;
                    }
                    if (fx_new[nOffspring] < base.fxopt)
                    {
                        base.fxopt = fx_new[nOffspring];
                        x_new[nOffspring].CopyTo(base.xopt, 0);
                    }


                    if (nOffspring + 1 < this.popsize)
                    {
                        x_new[nOffspring + 1] = child2;
                        fx_new[nOffspring + 1] = evalfnc(child2);
                        //Console.WriteLine("fx,x1,x2,{0},{1},{2}", fx_new[nOffspring + 1], child2[0], child2[1]);
                        base.evalcount++;
                        if (CheckIfNaN(fx_new[nOffspring + 1]))
                        {
                            return;
                        }
                        if (fx_new[nOffspring + 1] < base.fxopt)
                        {
                            base.fxopt = fx_new[nOffspring + 1];
                            x_new[nOffspring + 1].CopyTo(base.xopt, 0);
                        }
                        nOffspring++;
                    }
                    nOffspring++;
                } while (nOffspring < this.popsize);

                x_new.CopyTo(this.x_pop, 0);
                fx_new.CopyTo(this.fx_pop, 0);


                gen++;
            } while (gen < maxgen && base.evalcount < evalmax);
        }


        protected override void storeCurrentBest()
        {
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx_pop[p] < base.fxopt)
                {
                    base.fxopt = this.fx_pop[p];
                    this.x_pop[p].CopyTo(base.xopt, 0);
                }
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            bool stop = false;
            if (Double.IsNaN(fxtest))
            {
                //get the best
                storeCurrentBest();

                stop = true;
            }
            return stop;
        }


        /// <summary>
        /// initialize algorithm, i.e. initial population. Uniform random sampling over domain.
        /// </summary>
        private void initialize()
        {
            this.sumfitness = 0;

            int existing_p = this.x0.Length;
            this.fx0 = new double[this.popsize];
            if (this.x0.Length > 0)
            {
                double[][] _xpop = new double[this.popsize][];
                for (int p = 0; p < this.x0.Length; p++)
                {
                    _xpop[p] = new double[base.n];
                    this.x0.CopyTo(_xpop[p], 0);
                    this.fx0[p] = base.evalfnc(this.x0[p]);
                    //Console.WriteLine("fx,x1,x2,{0},{1},{2}", this.fx0[p], this.x0[p][0], this.x0[p][1]);
                    base.evalcount++;
                }
                this.x0 = new double[this.popsize][];   //i'm doing this, because x0.Length at initialisation could be < popsize
                _xpop.CopyTo(this.x0, 0);
            }
            else
            {
                this.x0 = new double[this.popsize][];
            }

            for (int p = existing_p; p < this.popsize; p++)
            {
                this.x0[p] = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    this.x0[p][i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i];
                    if (base.xint[i])
                    {
                        this.x0[p][i] = Math.Round(this.x0[p][i], 0);
                    }
                }
                this.fx0[p] = base.evalfnc(this.x0[p]);
                //Console.WriteLine("fx,x1,x2,{0},{1},{2}", this.fx0[p], this.x0[p][0], this.x0[p][1]);
                base.evalcount++;
            }


            double[] fitness_pop = CostToFitness(this.fx0);
            for (int p = 0; p < this.popsize; p++)
            {
                this.sumfitness += fitness_pop[p];
            }

            //get the best
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx0[p] < base.fxopt)
                {
                    base.fxopt = this.fx0[p];
                    this.x0[p].CopyTo(base.xopt, 0);
                }
            }
        }

        private double[] CostToFitness(double[] fx_pop)
        {
            int[] items = new int[this.popsize];
            double[] fitness = new double[this.popsize];
            double[] keys = new double[this.popsize];
            fx_pop.CopyTo(keys, 0);

            for (int p = 0; p < this.popsize; p++)
            {
                items[p] = p;
            }
            Array.Sort(keys, items);
            for (int p = 0; p < this.popsize; p++)
            {
                fitness[items[p]] = Convert.ToDouble(this.popsize - p);
            }

            return fitness;
        }


        /// <summary>
        /// Roulette wheel selection
        /// </summary>
        /// <param name="popsize"></param>
        /// <param name="sumfitness"></param>
        /// <param name="fitness_pop"></param>
        /// <returns></returns>
        private int select(double sumfitness, double[] fitness_pop)
        {
            int j = -1;
            double partsum = 0.0;
            double rand = base.rnd.NextDouble() * sumfitness;

            do
            {
                j++;
                partsum += fitness_pop[j];
            } while (partsum < rand && j != this.popsize - 1);

            return j;
        }



        private void crossovermutate(out double[] child1, out double[] child2, double[] parent1, double[] parent2, double d, double r, double k)
        {
            int jcross;  //crossover point

            child1 = new double[base.n];
            child2 = new double[base.n];

            for (int i = 0; i < base.n; i++)
            {
                if (base.rnd.NextDouble() <= this.pcross)
                {
                    if (base.xint[i])   //integer variable
                    {
                        if (this.lchrom[i] > 1)
                        {
                            jcross = base.rnd.Next(0, lchrom[i]);
                            string strP1 = Misc.Misc.Dec2Bin(Convert.ToInt32(parent1[i] - base.lb[i]), this.lchrom[i]);
                            string strP2 = Misc.Misc.Dec2Bin(Convert.ToInt32(parent2[i] - base.lb[i]), this.lchrom[i]);
                            char[] charP1 = strP1.ToCharArray();
                            char[] charP2 = strP2.ToCharArray();

                            char[] charC1 = new char[charP1.Length];
                            char[] charC2 = new char[charP1.Length];

                            for (int j = 0; j < jcross; j++)
                            {
                                charC1[j] = charP1[j];
                                charC2[j] = charP2[j];
                            }
                            for (int j = jcross; j < this.lchrom[i]; j++)
                            {
                                charC1[j] = charP2[j];
                                charC2[j] = charP1[j];
                            }
                            for (int j = 0; j < this.lchrom[i]; j++)
                            {
                                if (base.rnd.NextDouble() <= this.pmutation)
                                {
                                    if (charC1[j].Equals('0')) charC1[j] = '1';
                                    else charC1[j] = '0';
                                }
                                if (base.rnd.NextDouble() <= this.pmutation)
                                {
                                    if (charC2[j].Equals('0')) charC2[j] = '1';
                                    else charC2[j] = '0';
                                }
                            }

                            string strC1 = new string(charC1);
                            string strC2 = new string(charC2);
                            child1[i] = Misc.Misc.Bin2Dec(strC1) + base.lb[i];
                            if (child1[i] > base.ub[i]) child1[i] = base.ub[i]; //< lb not happening
                            child2[i] = Misc.Misc.Bin2Dec(strC2) + base.lb[i];
                            if (child2[i] > base.ub[i]) child2[i] = base.ub[i];
                        }
                        else    //swap 0 and 1.
                        {
                            child1[i] = parent2[i];
                            child2[i] = parent1[i];
                        }

                    }
                    else                //real variable... intermediate recombination
                    {
                        double alpha = this.rnd.NextDouble() * ((1 + d) + d) - d;
                        child1[i] = parent1[i] * alpha + parent2[i] * (1 - alpha);
                        child2[i] = parent2[i] * alpha + parent1[i] * (1 - alpha);


                        if (base.rnd.NextDouble() <= this.pmutation)
                        {
                            child1[i] = child1[i] + (base.rnd.NextDouble() * 2 - 1) * (r * (Math.Abs(base.ub[i] - base.lb[i]))) * (Math.Pow(2, (base.rnd.NextDouble() * -1) * k));
                            child2[i] = child2[i] + (base.rnd.NextDouble() * 2 - 1) * (r * (Math.Abs(base.ub[i] - base.lb[i]))) * (Math.Pow(2, (base.rnd.NextDouble() * -1) * k));
                        }
                    }
                }
                else
                {
                    child1[i] = parent1[i];
                    child2[i] = parent2[i];
                }

            }

            base.checkBounds(ref child1, base.lb, base.ub);
            base.checkBounds(ref child2, base.lb, base.ub);

        }


    }


    /// <summary>
    /// Simple Evolution Strategy
    /// Beyer and Schwefel (2002). Evolution strategies - A comprehensive introduction.
    /// </summary>
    public class SimpleES : SO_Solver
    {

        //(mu/roh ,+ lambda) - ES

        //Begin
        //g:=0;
        //initialize(Pop0:={(x0_i,s0_i,fx0_i), i=1,...,mu}
        //Repeat
        // For l:=1 to lambda Do Begin
        //     Fl := marriage(Pg, roh);
        //     sl := s_recombination(Fl);
        //     yl := y_recombination(Fl);
        //     stildel := s_mutation(sl);
        //     ytildel := y_mutation(yl, stildel);
        //     Ftildel := F(ytildel);
        //  End;
        //  P0g := {(ytildel, stildel, Ftildel), l=1,...,lambda};
        //  Case selection_type Of
        //     (mu , lambda) : Pg+1 := selection(P0g, mu);
        //     (mu + lambda) : Pg+1 := selection(P0g, Pg, mu);
        //  End;
        //  g := g+1;
        //Until termination_condition
        //End





        /// <summary>
        /// Population size
        /// </summary>
        public int popsize { get; private set; }


        /// <summary>
        /// cost values of new population
        /// </summary>
        public double[] fx_pop { get; private set; }
        /// <summary>
        /// decision variables of new population
        /// </summary>
        public double[][] x_pop { get; private set; }


        private double[][] x0;
        private double[] fx0;

        /// <summary>
        /// strategy parameters. step-size.
        /// </summary>
        private double[][] s;
        /// <summary>
        /// step-size s0 only used at initial sampling
        /// </summary>
        private double[] s0;

        /// <summary>
        /// learning rate
        /// </summary>
        private double tau0, tau, tauc;

        /// <summary>
        /// mu = parents, lambda = offspring, roh = mixing number
        /// </summary>
        private int lambda, roh;

        /// <summary>
        /// initiale population, sampling mode. 0=uniform sampling; 1=gaussian sampling around a base point.
        /// </summary>
        private int x0samplingmode;

        /// <summary>
        /// mutation probability per discrete variable x.
        /// </summary>
        private double pmut_int;

        /// <summary>
        /// selection mode for recombination. 0 = random, 1 = roulette wheel,... to do 2 = stochastic universal sampling
        /// </summary>
        private int selmode;

        public SimpleES(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[][] x0 = null)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.x0 = x0 ?? new double[0][];


            //popsize. same as mu
            if (settings.ContainsKey("popsize"))
                this.popsize = Convert.ToInt16(settings["popsize"]);
            else
                this.popsize = 20;


            //offspring
            if (settings.ContainsKey("lambda"))
                this.lambda = Convert.ToInt16(settings["lambda"]);
            else
                this.lambda = this.popsize;


            //mixing nr., i.e. how many parents involved in creating one offspring. roh=1 is only mutation.
            if (settings.ContainsKey("roh"))
                this.roh = Convert.ToInt16(settings["roh"]);
            else
                this.roh = 2;
            if (this.roh > this.popsize) this.roh = this.popsize;


            //selection mode for recombination. 0 = random, 1 = roulette, 2 = SUS 
            if (settings.ContainsKey("selmode"))
                this.selmode = Convert.ToInt16(settings["selmode"]);
            else
                this.selmode = 1;


            //stepsize s
            this.s = new double[this.popsize][];
            for (int p = 0; p < this.popsize; p++)
            {
                this.s[p] = new double[base.n];
                if (settings.ContainsKey("stepsize"))
                    for (int i = 0; i < base.n; i++)
                        this.s[p][i] = Convert.ToDouble(settings["stepsize"]);
                else
                    for (int i = 0; i < base.n; i++)
                        this.s[p][i] = 0.5;
            }


            //initial stepsize s0
            this.s0 = new double[base.n];
            if (settings.ContainsKey("stepsize0"))
                for (int i = 0; i < base.n; i++)
                    this.s0[i] = Convert.ToDouble(settings["stepsize0"]);
            else
                for (int i = 0; i < base.n; i++)
                    this.s0[i] = 0.5;


            //learning rate tau
            if (settings.ContainsKey("tauc"))
                this.tauc = Convert.ToDouble(settings["tauc"]);
            else
                this.tauc = 1;

            this.tau = this.tauc / (Math.Sqrt(2 * Math.Sqrt(base.n)));
            this.tau0 = this.tauc / (Math.Sqrt(2 * base.n));


            //mutation probability, only for integer
            if (settings.ContainsKey("pmut_int"))
                this.pmut_int = Convert.ToDouble(settings["pmut_int"]);
            else
                this.pmut_int = 0.5;









            if (settings.ContainsKey("x0sampling"))
            {
                this.x0samplingmode = Convert.ToInt16(settings["x0sampling"]);
                if (this.x0samplingmode > 1) this.x0samplingmode = 1;               //gaussian sampling around a point, which is uniformly sampled (unless an x0 is given)
            }
            else
            {
                this.x0samplingmode = 0;    //uniform sampling within the search domain
            }

        }


        private void initialize()
        {
            int existing_p = this.x0.Length;
            this.fx0 = new double[this.popsize];
            bool x0exists = false;
            if (this.x0.Length > 0)
            {
                x0exists = true;
                double[][] _xpop = new double[this.popsize][];
                for (int p = 0; p < this.x0.Length; p++)
                {
                    _xpop[p] = new double[base.n];
                    this.x0[p].CopyTo(_xpop[p], 0);
                    this.fx0[p] = base.evalfnc(this.x0[p]);
                    this.evalcount++;
                }
                this.x0 = new double[this.popsize][];   //i'm doing this, because x0.Length at initialisation could be < popsize
                _xpop.CopyTo(this.x0, 0);
            }
            else
            {
                this.x0 = new double[this.popsize][];
            }

            double[] xbasepoint = new double[base.n];
            if (x0exists)
            {
                this.x0[0].CopyTo(xbasepoint, 0);       //always take the first entry, no matter if more than 1 has been inputted
            }
            else
            {
                for (int i = 0; i < base.n; i++)
                {
                    xbasepoint[i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i]; //if no x0 exists, uniform sampling
                    if (base.xint[i])
                    {
                        xbasepoint[i] = Math.Round(xbasepoint[i], 0);
                    }
                }
            }
            for (int p = existing_p; p < this.popsize; p++)
            {
                this.x0[p] = new double[base.n];
                if (this.x0samplingmode == 0)
                {
                    // uniform sampling withing the search domain
                    for (int i = 0; i < base.n; i++)
                    {
                        this.x0[p][i] = base.rnd.NextDouble() * (base.ub[i] - base.lb[i]) + base.lb[i];
                        if (base.xint[i])
                        {
                            this.x0[p][i] = Math.Round(this.x0[p][i], 0);
                        }
                    }
                }
                else
                {
                    //gaussian sampling around a point
                    for (int i = 0; i < base.n; i++)
                    {
                        this.x0[p][i] = (base.rnd.NextGaussian(0, this.s0[i]) * (base.ub[i] - base.lb[i])) + xbasepoint[i];
                        //this.x0[p][i] = (base.rnd.NextGaussian(0, this.s0[i]) * (base.ub[i] - base.lb[i])) + xbasepoint[i];
                        if (base.xint[i])
                        {
                            this.x0[p][i] = Math.Round(this.x0[p][i], 0);
                        }
                    }
                }
                base.checkBounds(ref this.x0[p], base.lb, base.ub);
                this.fx0[p] = base.evalfnc(this.x0[p]);
                this.evalcount++;
            }

            //get the best
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx0[p] < base.fxopt)
                {
                    base.fxopt = this.fx0[p];
                    this.x0[p].CopyTo(base.xopt, 0);
                }
            }


            this.x_pop = new double[this.popsize][];
            this.x0.CopyTo(this.x_pop, 0);
            this.fx_pop = new double[this.popsize];
            this.fx0.CopyTo(this.fx_pop, 0);

            Array.Sort(this.fx_pop, this.x_pop);
        }


        public override void solve()
        {
            this.initialize();

            //Begin
            //g:=0;     dont need generations, im using eval count
            while (base.evalcount < base.evalmax)
            {
                int[] int_family;
                double[][] s_new = new double[this.lambda][];
                double[][] x_new = new double[this.lambda][];
                double[] fx_new = new double[this.lambda];
                for (int l = 0; l < this.lambda; l++)
                {
                    this.marriage(out int_family, this.popsize, this.roh, this.selmode);
                    this.s_recombination(out s_new[l], this.s, int_family);
                    this.x_recombination(out x_new[l], this.x_pop, int_family);   //interm. recomb. for R, or coordinate-wise recomb for N
                    this.s_mutation(ref s_new[l], this.tau);
                    this.x_mutation(ref x_new[l], s_new[l], this.tau);
                    fx_new[l] = base.evalfnc(x_new[l]);
                    base.evalcount++;
                    if (this.CheckIfNaN(fx_new[l]))
                    {
                        if (fx_new[l] < base.fxopt)
                        {
                            base.fxopt = fx_new[l];
                            x_new[l].CopyTo(base.xopt, 0);
                        }
                        return;
                    }
                }
                double[][] x_sel;
                double[] fx_sel;
                double[][] s_sel;
                this.selection(out x_sel, out fx_sel, out s_sel, this.x_pop, this.fx_pop, this.s, x_new, fx_new, s_new);
                x_sel.CopyTo(this.x_pop, 0);
                fx_sel.CopyTo(this.fx_pop, 0);
                s_sel.CopyTo(this.s, 0);
                this.storeCurrentBest();
                //Console.WriteLine("eval: {0} with fx: {1}", base.evalcount, base.get_fxoptimum());
            }
        }


        /// <summary>
        /// pure random selection. No fitness-proportionate selection.
        /// </summary>
        /// <param name="_xfamily"></param>
        /// <param name="_sfamily"></param>
        /// <param name="_xpop"></param>
        /// <param name="_spop"></param>
        /// <param name="_roh"></param>
        private void marriage(out int[] _intfamily, int _popsize, int _roh, int selmode)
        {
            _intfamily = new int[this.roh];
            bool[] selected = new bool[_popsize];

            switch (selmode)
            {
                case 1: //roulette
                    List<int> lengths = new List<int>();
                    for (int p = 0; p < _popsize; p++) lengths.Add(_popsize - p); //first entry gets biggest value
                    for (int i = 0; i < this.roh; i++)
                    {
                        int ranksum = lengths.Sum();
                        int rand = base.rnd.Next(ranksum);
                        int sel = -1;
                        int partsum = 0;
                        do
                        {
                            sel++;
                            partsum += lengths[sel];
                        } while (partsum < rand && sel != _popsize - 1);

                        _intfamily[i] = popsize - lengths[sel];
                        lengths.RemoveAt(sel);
                    }
                    break;
                default:    //random, no fitness proportionate
                    List<int> indices = new List<int>();
                    for (int p = 0; p < _popsize; p++) indices.Add(p);
                    for (int i = 0; i < this.roh; i++)
                    {
                        int rndind = rnd.Next(indices.Count());
                        int sel = indices[rndind];
                        indices.RemoveAt(rndind);
                        _intfamily[i] = sel;
                    }
                    break;
            }

        }

        private void s_recombination(out double[] _s_new, double[][] _s, int[] _intfamily)
        {
            int _roh = _intfamily.Length;
            _s_new = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                for (int u = 0; u < _roh; u++) //length of roh, mixing number
                {
                    _s_new[i] += _s[_intfamily[u]][i];
                }
                _s_new[i] /= _roh;
            }
        }

        /// <summary>
        /// intermediate recombination for Real-valued. coordinate-wise random selection for discrete parameters.
        /// </summary>
        /// <param name="_x_new"></param>
        /// <param name="_intfamily"></param>
        private void x_recombination(out double[] _x_new, double[][] _xpop, int[] _intfamily)
        {
            int _roh = _intfamily.Length;
            _x_new = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                if (base.xint[i])   //integer
                {
                    _x_new[i] = _xpop[_intfamily[rnd.Next(roh)]][i];
                }
                else                //real
                {
                    for (int u = 0; u < _roh; u++) //length of roh, mixing number
                    {
                        _x_new[i] += _xpop[_intfamily[u]][i];
                    }
                    _x_new[i] /= _roh;
                }
            }
        }


        private void s_mutation(ref double[] _snew, double _tau)
        {
            double tau0exp = Math.Exp(this.tau0 * rnd.NextGaussian(0, 1));
            for (int i = 0; i < base.n; i++)
            {
                _snew[i] = tau0exp * _snew[i] * Math.Exp(this.tau * rnd.NextGaussian(0, 1));
            }
        }

        private void x_mutation(ref double[] _xnew, double[] _snew, double _tau)
        {
            for (int i = 0; i < base.n; i++)
            {
                if (base.xint[i])
                {
                    if (rnd.NextDouble() < this.pmut_int)
                    {
                        _xnew[i] = Convert.ToDouble(rnd.Next(Convert.ToInt16(base.lb[i]), Convert.ToInt16(base.ub[i]) + 1));
                    }
                }
                else
                {
                    _xnew[i] = _xnew[i] + (_snew[i] * rnd.NextGaussian(0, 1));
                }
            }
            base.checkBounds(ref _xnew, base.lb, base.ub);
        }

        private void selection(out double[][] _x_sel, out double[] _fx_sel, out double[][] _s_sel,
            double[][] _x_old, double[] _fx_old, double[][] _s_old, double[][] _x_new, double[] _fx_new, double[][] _s_new)
        {
            int popsize = _x_old.Length;
            int lambda = _x_new.Length;

            double[][] _x_merged = new double[popsize + lambda][];
            double[] _fx_merged = new double[popsize + lambda];
            double[][] _s_merged = new double[popsize + lambda][];
            _x_old.CopyTo(_x_merged, 0);
            _x_new.CopyTo(_x_merged, popsize);
            _fx_old.CopyTo(_fx_merged, 0);
            _fx_new.CopyTo(_fx_merged, popsize);
            _s_old.CopyTo(_s_merged, 0);
            _s_new.CopyTo(_s_merged, popsize);

            _x_old = new double[popsize][];
            _fx_old = new double[popsize];
            _s_old = new double[popsize][];
            double[][][] _xands = new double[popsize + lambda][][];
            for (int i = 0; i < _xands.Length; i++)
                _xands[i] = new double[2][];

            for (int i = 0; i < popsize + lambda; i++)
            {
                _xands[i][0] = new double[_x_merged[0].Length];
                _xands[i][1] = new double[_s_merged[0].Length];
                _x_merged[i].CopyTo(_xands[i][0], 0);
                _s_merged[i].CopyTo(_xands[i][1], 0);
            }

            //rank according to fitness
            Array.Sort(_fx_merged, _xands);

            _x_sel = new double[popsize][];
            _fx_sel = new double[popsize];
            _s_sel = new double[popsize][];
            for (int i = 0; i < popsize; i++)
            {
                _x_sel[i] = new double[_x_merged[0].Length];
                _xands[i][0].CopyTo(_x_sel[i], 0);

                _fx_sel[i] = _fx_merged[i];

                _s_sel[i] = new double[_s_merged[0].Length];
                _xands[i][1].CopyTo(_s_sel[i], 0);
            }
        }

        protected override void storeCurrentBest()
        {
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx_pop[p] < base.fxopt)
                {
                    base.fxopt = this.fx_pop[p];
                    this.x_pop[p].CopyTo(base.xopt, 0);
                }
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            bool stop = false;
            if (Double.IsNaN(fxtest))
            {
                //get the best
                this.storeCurrentBest();

                stop = true;
            }
            return stop;
        }

    }


    /// <summary>
    /// Nelder-Mead Simplex.
    ///  <para/>Nelder J.A. and Mead R. (1965). A Simplex method for function minimization.
    ///  <para/>Lagarias J.C., Reeds J.A., Wright M.H. and Wright P.E. (1998). Convergence Properties of the Nelder-Mead Simplex Method in Low Dimensions.
    ///  <para/>Based on source code from: MATLAB 2014 fmnisearch.m
    ///  <para/>Description: Local hill-climber. Derivative-free direct search method. Constructs a n+1 simplex, whose vertices are reflected, expanded and contracted. 
    /// </summary>
    public class NelderMead : SO_Solver
    {
        /// <summary>
        /// Population size. In Nelder Mead it is n+1
        /// </summary>
        public int popsize { get; private set; }
        /// <summary>
        /// cost values of new population
        /// </summary>
        public double[] fx_pop { get; private set; }
        /// <summary>
        /// decision variables of new population
        /// </summary>
        public double[][] x_pop { get; private set; }


        private double[] x0;
        private double fx0;

        /// <summary>
        /// Strategy parameters for reflection, expansion, contraction, and shrinkage coefficients.
        /// </summary>
        private double alpha, gamma, rho, sigma;
        /// <summary>
        /// Stepsize for initial sampling, as standard deviation of a normal distribution around a point
        /// </summary>
        private double step0;

        public NelderMead(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[] x0 = null)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.x0 = x0;//?? new double[base.n];

            //popsize always n+1
            this.popsize = base.n + 1;

            //reflection
            if (settings.ContainsKey("alpha"))
                this.alpha = Convert.ToDouble(settings["alpha"]);
            else
                this.alpha = 1;

            //expansion
            if (settings.ContainsKey("gamma"))
                this.gamma = Convert.ToDouble(settings["gamma"]);
            else
                this.gamma = 1.5;

            //contraction
            if (settings.ContainsKey("rho"))
                this.rho = Convert.ToDouble(settings["rho"]);
            else
                this.rho = 0.25;

            //shrinkage
            if (settings.ContainsKey("sigma"))
                this.sigma = Convert.ToDouble(settings["sigma"]);
            else
                this.sigma = 0.2;

            //stepsize for initial sampling
            if (settings.ContainsKey("step0"))
                this.step0 = Convert.ToDouble(settings["step0"]);
            else
                this.step0 = 0.02;
        }

        private void initialize()
        {
            this.x_pop = new double[this.popsize][];
            this.fx_pop = new double[this.popsize];

            double[] center = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                center[i] = (base.lb[i] + base.ub[i]) / 2;
            }

            if (this.x0 == null || this.x0.Length == 0)
            {
                // sample new x0
                //gaussian sampling around a point
                this.x0 = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    this.x0[i] = (base.rnd.NextGaussian(0, this.step0) * (base.ub[i] - base.lb[i])) + center[i];
                    if (base.xint[i])
                    {
                        this.x0[i] = Math.Round(this.x0[i], 0);
                    }
                }
            }
            base.checkBounds(ref this.x0, base.lb, base.ub);
            this.x_pop[0] = new double[base.n];
            this.x0.CopyTo(this.x_pop[0], 0);
            this.fx_pop[0] = base.evalfnc(this.x_pop[0]);
            base.evalcount++;

            for (int p = 1; p < this.popsize; p++)
            {
                this.x_pop[p] = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    this.x_pop[p][i] = (base.rnd.NextGaussian(0, this.step0) * (base.ub[i] - base.lb[i])) + this.x0[i];
                    if (base.xint[i])
                    {
                        this.x_pop[p][i] = Math.Round(this.x_pop[p][i], 0);
                    }
                }
                base.checkBounds(ref this.x_pop[p], base.lb, base.ub);
                this.fx_pop[p] = base.evalfnc(this.x_pop[p]);
                base.evalcount++;
            }

            Array.Sort(this.fx_pop, x_pop);
            this.storeCurrentBest();
        }


        public override void solve()
        {
            this.initialize();


            while (base.evalcount < base.evalmax)
            {
                double[][] Simplex_cop = this.x_pop.ToArray();
                double[][] Simplex = new double[Simplex_cop.Length][];
                for (int i = 0; i < Simplex.Length; i++)
                {
                    Simplex[i] = new double[Simplex_cop[i].Length];
                    Simplex_cop[i].CopyTo(Simplex[i], 0);
                }

                double[] SimplexObjval = this.fx_pop.ToArray();


                // create xc - centroid of all vertices, except n+1
                double[] xc = this.NM_centroid(Simplex);

                // reflect, expand, contract, shrink
                double[] xr = this.NM_Reflect(Simplex[base.n], xc, this.alpha);

                // evaluate xr
                for (int i = 0; i < base.n; i++)
                {
                    if (xr[i] < 0) xr[i] = 0;
                    else if (xr[i] > 1) xr[i] = 1;
                }

                double xrObjval = base.evalfnc(xr);
                base.evalcount++;

                // check, what to do and when to end this loop
                // if its better then the 2nd worst but not better then the best,
                if (xrObjval < SimplexObjval[base.n - 1] && xrObjval > SimplexObjval[0])
                {
                    //replace the worst and start from beginning ->end sub
                    this.NM_replaceVertex(xr, xrObjval, ref Simplex, ref SimplexObjval);
                }
                // if its the new best
                else if (xrObjval < SimplexObjval[0])
                {
                    // expand the reflected point further
                    double[] xs = this.NM_expand(xr, xc, gamma);
                    this.checkBounds(ref xs, base.lb, base.ub);
                    double xsObjval = base.evalfnc(xs);
                    base.evalcount++;

                    //if expanded point is better, then reflected
                    if (xsObjval < xrObjval)
                    {
                        // take it as new one to replace the current worst
                        this.NM_replaceVertex(xs, xsObjval, ref Simplex, ref SimplexObjval);
                    }
                    else
                    {
                        // keep the reflected point 
                        this.NM_replaceVertex(xr, xrObjval, ref Simplex, ref  SimplexObjval);
                    }
                }
                // if reflected is worse then 2nd worst
                else if (xrObjval > SimplexObjval[base.n - 1])
                {
                    double[] xcon = new double[base.n];
                    double xconObjval = 0;
                    bool shrink = false;
                    // if reflected is better then current worst
                    if (xrObjval < SimplexObjval[base.n])
                    {
                        //contract outside
                        this.NM_contract(ref xcon, ref xconObjval, ref shrink, xr, xrObjval, xc, rho);
                    }
                    // if reflected is worse then current worst
                    else if (xrObjval >= SimplexObjval[base.n])
                    {
                        // contract inside
                        this.NM_contract(ref xcon, ref xconObjval, ref shrink, Simplex[base.n], SimplexObjval[base.n], xc, rho);
                    }
                    if (shrink == true)
                    {
                        this.NM_shrink(ref Simplex, ref SimplexObjval, sigma);
                    }
                    else
                    {
                        this.NM_replaceVertex(xcon, xconObjval, ref Simplex, ref SimplexObjval);
                    }
                }
                //e.g. if its worse then the worst or has the same objval as the worst
                else
                {
                    Console.WriteLine("Degeneracy! at evalfunccalls {0}", base.evalcount);
                    return;
                }
                Array.Sort(SimplexObjval, Simplex);
                this.x_pop = new double[this.popsize][];//Simplex);
                this.fx_pop = new double[this.popsize]; //(SimplexObjval);
                Simplex.CopyTo(this.x_pop, 0);
                SimplexObjval.CopyTo(this.fx_pop, 0);



                this.storeCurrentBest();
            }

        }

        protected override void storeCurrentBest()
        {
            for (int p = 0; p < this.popsize; p++)
            {
                if (this.fx_pop[p] < base.fxopt)
                {
                    base.fxopt = this.fx_pop[p];
                    this.x_pop[p].CopyTo(base.xopt, 0);
                }
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            bool stop = false;
            if (Double.IsNaN(fxtest))
            {
                //get the best
                this.storeCurrentBest();

                stop = true;
            }
            return stop;
        }

        private double[] NM_centroid(double[][] Simplex)
        {
            double[][] SimplexCen = new double[Simplex.Length - 1][];
            for (int i = 0; i < Simplex.Length - 1; i++)
            {
                SimplexCen[i] = new double[base.n];
                SimplexCen[i] = Simplex[i];
            }
            return Misc.Vector.Centroid(SimplexCen);
        }

        private double[] NM_Reflect(double[] xLast, double[] xc, double alpha)
        {
            //r = 2xc -x(n+1)
            //xc = sum(x(i))/n .... the centroid 
            double[] xr = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                xr[i] = xc[i] + alpha * (xc[i] - xLast[i]);
            }

            return xr;
        }

        private void NM_replaceVertex(double[] xReplace, double objValReplace, ref double[][] Simplex, ref double[] SimplexObjval)
        {
            Simplex[base.n] = xReplace;
            SimplexObjval[base.n] = objValReplace;
        }

        private double[] NM_expand(double[] xReflected, double[] xc, double gamma)
        {
            //xs = xc + 2(xc -x(n+1))
            //xc = sum(x(i))/n .... the centroid 
            double[] xs = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                xs[i] = xc[i] + gamma * (xReflected[i] - xc[i]);
            }

            return xs;
        }

        private void NM_contract(ref double[] xcon, ref double xconObjval, ref bool shrink,
           double[] xCompare, double xCompareObjval, double[] xc, double rho)
        {
            for (int i = 0; i < base.n; i++)
            {
                xcon[i] = xc[i] + rho * (xCompare[i] - xc[i]);
            }
            this.checkBounds(ref xcon, base.lb, base.ub);
            xconObjval = base.evalfnc(xcon);
            base.evalcount++;

            // outside: if the contracted point is better then the reflected, take it, otherwise shrink
            // inside: if the contracted point is better then the worst, take it, otherwise shrink
            if (xconObjval < xCompareObjval)
            {
                shrink = false;
            }
            else
            {
                shrink = true;
            }
        }

        private void NM_shrink(ref double[][] Simplex, ref double[] SimplexObjval, double sigma)
        {
            // keep the best vertex
            // shrink the rest
            for (int i = 1; i < Simplex.Length; i++)
            {
                double[] xShrinked = new double[base.n];
                for (int j = 0; j < base.n; j++)
                {
                    xShrinked[j] = Simplex[0][j] + sigma * (Simplex[i][j] - Simplex[0][j]);
                }
                this.checkBounds(ref xShrinked, base.lb, base.ub);
                double fxShrinked = base.evalfnc(xShrinked);
                base.evalcount++;

                Simplex[i] = new double[base.n];
                xShrinked.CopyTo(Simplex[i], 0);
                SimplexObjval[i] = fxShrinked;
            }
        }
    }


    /// <summary>
    /// Rosenbrock Search Algorithm.
    /// <para/>Rosenbrock H.H. (1960). An Automatic Method for finding the Greatest or Least Value of a Function.
    /// <para/>Palmer J.R. (1969). An improved procedure for orthogonalising the search vectors in Rosenbrock's and Swann's direct search optimisation methods.
    /// <para/>Based on source code from: Kniaz.net 
    /// <para/>Description: Local optimizer. Conducts orthogonal steps in a coordinate system that is rotated in order to approximate the gradient.
    /// </summary>
    public class Rosenbrock : SO_Solver
    {

        private double[] x0;
        private double fx0;
        private double[] xtest;
        private double fxtest;


        //Rosenbrock variables
        private bool[] flag_success;    //for every directon (dimension), indicating if there was a successfull move in the iteration
        private bool[] flag_fail;       //same, but for fail moves
        private int flags_sum;          // summing up TRUEs. so I dont always need to loop through all flags to check. I know: sum=2n, then all true
        private int flag_currentDirection; //indicating, in which direction to move in this move-iteration
        private double[] xOrigin;       //starting point of iteration (i.e. until all flags fullfilled). then xOrigin changes xBest - xOrigin
        private double[][] vMoves;      //n orthogonal vectors


        /// <summary>
        /// Strategy parameters for extension, shrinking, and initial steplength
        /// </summary>
        private double alpha, beta, stepsize;

        public Rosenbrock(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[] x0 = null)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            this.x0 = x0;//?? new double[base.n];


            //extension
            if (settings.ContainsKey("alpha"))
                this.alpha = Convert.ToDouble(settings["alpha"]);
            else
                this.alpha = 2;

            //shrinking
            if (settings.ContainsKey("beta"))
                this.beta = Convert.ToDouble(settings["beta"]);
            else
                this.beta = 0.5;

            //initial step size
            if (settings.ContainsKey("stepsize"))
                this.stepsize = Convert.ToDouble(settings["stepsize"]);
            else
                this.stepsize = 0.125;


        }


        private void initialize()
        {
           
            double[] center = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                center[i] = (base.lb[i] + base.ub[i]) / 2;
            }

            if (this.x0 == null || this.x0.Length == 0)
            {
                // sample new x0
                //gaussian sampling around a point
                this.x0 = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    this.x0[i] = (base.rnd.NextGaussian(0, this.stepsize) * (base.ub[i] - base.lb[i])) + center[i];
                    if (base.xint[i])
                    {
                        this.x0[i] = Math.Round(this.x0[i], 0);
                    }
                }
            }
            base.checkBounds(ref this.x0, base.lb, base.ub);
            this.xtest = new double[base.n];
            this.x0.CopyTo(this.xtest, 0);
            this.fx0 = base.evalfnc(this.xtest);
            this.fxtest = this.fx0;
            base.evalcount++;

            this.xtest = this.NormalizeX(this.xtest);


            Console.WriteLine("x0, x1, fx, {0},{1},{2}", this.ReverseNormalization(this.xtest)[0], this.ReverseNormalization(this.xtest)[1], this.fxtest);
            this.storeCurrentBest();






            //Initialize Rosenbrock strategy parameters and variables
            List<bool[]> rb_flag_fail = new List<bool[]>();
            List<bool[]> rb_flag_success = new List<bool[]>();
            List<int> rb_flags_sum = new List<int>();
            List<int> rb_flag_currentDirection = new List<int>();
            List<double[][]> rb_vMoves = new List<double[][]>();
            List<double[]> rb_xOrigin = new List<double[]>();

            I_Rosenbrock(ref rb_flag_fail, ref rb_flag_success, ref rb_flags_sum, ref rb_flag_currentDirection,
               ref rb_vMoves, ref rb_xOrigin, this.stepsize);

            this.flag_fail = rb_flag_fail[0];
            this.flag_success = rb_flag_success[0];
            this.flags_sum = rb_flags_sum[0];
            this.flag_currentDirection = rb_flag_currentDirection[0];
            this.vMoves = rb_vMoves[0];

            this.xOrigin = new double[base.n];
            rb_xOrigin[0].CopyTo(this.xOrigin, 0);
        }


        public override void solve()
        {
            this.initialize();

            while (base.evalcount < base.evalmax)
            {
                this.M_Rosenbrock(ref this.flag_fail, ref this.flag_success, ref this.flags_sum, ref this.flag_currentDirection, ref this.xOrigin, ref this.vMoves, this.alpha, this.beta, this.stepsize);
                if (this.CheckIfNaN(this.fxtest)) return;
                this.storeCurrentBest();
            }
        }

        protected override void storeCurrentBest()
        {
            if (this.fxtest < base.fxopt)
            {
                base.xopt = new double[n];
                this.ReverseNormalization(this.xtest).CopyTo(base.xopt, 0);
                base.fxopt = this.fxtest;
            }
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            if (Double.IsNaN(fxtest))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rosenbrock moves.
        /// </summary>
        /// <param name="flag_fail"></param>
        /// <param name="flag_success"></param>
        /// <param name="flags_sum"></param>
        /// <param name="flag_currentDirection"></param>
        /// <param name="xOrigin"></param>
        /// <param name="vMoves"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        private void M_Rosenbrock(ref bool[] flag_fail, ref bool[] flag_success,
            ref int flags_sum, ref int flag_currentDirection,
            ref double[] xOrigin, ref double[][] vMoves,
            double alpha, double beta, double steplength)
        {
            // **********************************************************************************
            // 0.
            // **********************************************************************************
            //check if 4 conditions fullfilled to make new orthogonaliztion
            if (flags_sum == 2 * base.n)
            {
                double[] basevector = new double[base.n];
                for (int i = 0; i < base.n; i++)
                {
                    basevector[i] = xOrigin[i] - this.NormalizeX(base.xopt)[i];
                }
                Array.Copy(this.NormalizeX(base.xopt), xOrigin, base.n);   //new origin point
                double[][] vMoves_copy = Transformation.GramSchmidt(basevector, base.n, vMoves);     //new move vectors
                for (int i = 0; i < base.n; i++)
                {
                    for (int u = 0; u < base.n; u++)
                    {
                        vMoves_copy[i][u] *= steplength;
                    }
                }
                vMoves_copy.CopyTo(vMoves, 0);

                flag_fail = new bool[base.n];
                flag_success = new bool[base.n];
                flags_sum = 0;
                flag_currentDirection = 0;        //dunno, should this be also reseted?
            }
            //if yes, make a vector between xbase(previous starting point) and xbest(current best value)
            //use this as base vector to make gram-schmidt. basically, is the eigenvector




            // **********************************************************************************
            // 1.
            // **********************************************************************************
            // n-searches sequentially in direction of base vectors
            //      so 1st search only change xTest = x0 + [Alpha, 0, 0, ..., 0n]
            //      then 2nd search           xTest' = xTest + [0, Alpha, 0, ..., 0n]
            //      and that for every n
            if (flag_currentDirection == base.n) flag_currentDirection = 0;

            for (int i = 0; i < base.n; i++)
            {
                this.xtest[i] = this.NormalizeX(base.xopt)[i] + vMoves[flag_currentDirection][i];
                if (this.xtest[i] < 0) this.xtest[i] = 0;
                else if (this.xtest[i] > 1) this.xtest[i] = 1;
            }
            this.fxtest = base.evalfnc(this.ReverseNormalization(this.xtest));
            base.evalcount++;
            Console.WriteLine("x0, x1, fx, {0},{1},{2}", this.ReverseNormalization(this.xtest)[0], this.ReverseNormalization(this.xtest)[1], this.fxtest);
            if (this.CheckIfNaN(this.fxtest)) return;


            if (this.fxtest < base.fxopt)
            {
                //its better, so take it and increase stepsize with alpha
                for (int i = 0; i < base.n; i++)
                    vMoves[flag_currentDirection][i] *= alpha;

                //if flag is not 1 already, make it 1 and add to flags_sum
                if (flag_success[flag_currentDirection] != true)
                {
                    flag_success[flag_currentDirection] = true;
                    flags_sum++;
                }

                //make it new best point
                Array.Copy(this.ReverseNormalization(this.xtest), base.xopt, base.n);
                base.fxopt = this.fxtest;
            }
            else
            {
                //its worse, so reverse vMove for this direction and contract steplength with beta
                for (int i = 0; i < base.n; i++)
                    vMoves[flag_currentDirection][i] *= -1 * beta;

                //if fail flag not 1, make it 1 an add to flags sum
                if (flag_fail[flag_currentDirection] != true)
                {
                    flag_fail[flag_currentDirection] = true;
                    flags_sum++;
                }
            }



            flag_currentDirection++;
        }

        /// <summary>
        /// initializes and returns stuff for Rosenbrock moves
        /// </summary>
        /// <param name="flag_fail"></param>
        /// <param name="flag_success"></param>
        /// <param name="flags_sum"></param>
        /// <param name="flag_currentDirection"></param>
        /// <param name="vMoves"></param>
        /// <param name="xOrigin"></param>
        /// <param name="steplength"></param>
        private void I_Rosenbrock(ref List<bool[]> rb_flag_fail, ref List<bool[]> rb_flag_success,
            ref List<int> rb_flags_sum, ref List<int> rb_flag_currentDirection,
            ref List<double[][]> rb_vMoves, ref List<double[]> rb_xOrigin, double steplength)
        {
            rb_flag_fail = new List<bool[]>();
            rb_flag_success = new List<bool[]>();
            rb_flags_sum = new List<int>();
            rb_flag_currentDirection = new List<int>();
            rb_vMoves = new List<double[][]>();
            rb_xOrigin = new List<double[]>();

            //conditions for new orthogonalization
            bool[] _flag_fail = new bool[base.n];
            bool[] _flag_success = new bool[base.n];
            int _flags_sum = 0;
            int _flag_currentDirection = 0;
            double[][] _vMoves = new double[base.n][];
            double[] _xOrigin = new double[base.n];


            //origin point for current iteration( base)
            rb_xOrigin.Add(this.xtest);

            //initial orthogonal move vectors
            //rb_vMoves[i] = new double[this.base.n][];
            for (int u = 0; u < base.n; u++)
            {
                _vMoves[u] = new double[base.n];
                for (int k = 0; k < base.n; k++)
                {
                    if (u == k) _vMoves[u][k] = steplength;
                    else _vMoves[u][k] = 0;
                }
            }




            rb_flag_fail.Add(_flag_fail);
            rb_flag_success.Add(_flag_success);
            rb_flags_sum.Add(_flags_sum);
            rb_flag_currentDirection.Add(_flag_currentDirection);
            rb_vMoves.Add(_vMoves);

        }


        private double[] NormalizeX(double[] _x)
        {
            double[] _xNorm = new double[_x.Length];
            //Array.Copy(x0, xTest, base.n);
            for (int i = 0; i < base.n; i++)
            {
                _xNorm[i] = (_x[i] - base.lb[i]) / Math.Abs(base.ub[i] - base.lb[i]);
            }
            return _xNorm;
        }

        private double[] ReverseNormalization(double[] _xNorm)
        {
            double[] _x = new double[_xNorm.Length];
            for (int i = 0; i < _xNorm.Length; i++)
            {
                _x[i] = Math.Abs(base.ub[i] - base.lb[i]) * _xNorm[i] + base.lb[i];
            }
            return _x;
        }
    }

    /// <summary>
    /// DIRECT.
    /// <para/>DIviding RECTangles algorithm.
    /// <para/>Jones, Perttunen, Stuckman (1993). Lipschitzian Optimization Without the Lipschitz Constant.
    /// <para/>Based on Matlab code: http://www4.ncsu.edu/~ctk/Finkel_Direct/
    /// </summary>
    public class Direct : SO_Solver
    {

        public bool[] xInteger { get; private set; }    //indicating, which variable is an Integer. 0:double, 1:Integer

        
        // parameters DIRECT
        double par_ep = 1e-4;
        bool par_testflag = false;                      // terminate if within a relative tolerence of f_opt?
        double par_globalmin = 0;                       // minimum value of function. if known.
        


        double[] thirds = new double[100];              // thirds of the hypercube. just an array storing the side lengths of thirds of thirds of thirds...
        List<int>[] lengths;                            // # of slices per dimension-direction. one slice means division into 3, or 2 cuts. so 1 means the hypercube is cut into 3 parts in that dimension
        List<double> szes = new List<double>();         // size of each hyperrectangle. its the normvector ||x||, x∈{x1,x2,x3,...}. each vector is measured perpendicular from center of hyperrectangle to its edge
        List<double>[] c;                               // center coordinates of all hyperrectangles. array of list, because each element per variable dimension. list then extended if new boxes are added
        List<double> fc = new List<double>();           // function values at each box
        double minval;                                  // minimum function value so far
        double[] xatmin;                                // x of the best (minval) so far
        double perror;                                  // error... difference to expected minimum




        List<Tuple<int, int, double>> history = new List<Tuple<int, int, double>>();       // history. item1:  iteration, item2: function evaluations, item3: min value at that iteration
        Dictionary<int, double> S;                      // Pareto optimal solutions per iteration. int=> index of box, double=>size of box (sorted)


        double fxAbort;     //if NaN, then abort


        public Direct(double[] lb, double[] ub, bool[] xint, int evalmax, Func<double[], double> evalfnc, int seed)
            : base(lb, ub, xint, evalmax, evalfnc, seed)
        {
            
        }


        private void initialize()
        {
            this.DIRini();
            if (this.CheckIfNaN(this.fxAbort)) return;
            this.S = new Dictionary<int, double>();  // purge List of potential boxes. 1 is index (make (int)...), 2 is szes size of box
            this.find_po();          // find pareto optimal boxes... dictionary S
        }


        public override void solve()
        {
            this.initialize();
            if (this.CheckIfNaN(this.fxAbort)) return;

            while (base.evalcount < base.evalmax)
            {
                // %-- Loop through the potentially optimal hrectangles -----------%
                // %-- and divide -------------------------------------------------%
                foreach (KeyValuePair<int, double> potentialBox in this.S)
                {
                    this.DIRdivide(potentialBox.Key);
                    if (this.CheckIfNaN(this.fxAbort)) return;
                }

                this.DIRbest();
                S = new Dictionary<int, double>();  // purge List of potential boxes. 1 is index (make (int)...), 2 is szes size of box
                find_po();          // find pareto optimal boxes... dictionary S


            }
        }

        protected override void storeCurrentBest()
        {
            // being taken care of in DIRbest()
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            if (Double.IsNaN(fxtest))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        void DIRbest()
        {
            //            %-- update minval, xatmin --------------------------------------%
            double min = fc[0];
            int minIndex = 0;

            for (int i = 1; i < fc.Count; i++)
            {
                if (fc[i] < min)
                {
                    min = fc[i];
                    minIndex = i;
                }
            }
            minval = fc[minIndex];

            for (int u = 0; u < base.n; u++)
            {
                xatmin[u] = c[u][minIndex];
            }
            

            base.xopt = xatmin;
            base.fxopt = fc[minIndex];
        }



        /// <summary>
        /// Divides rectangle i that is passed in
        /// </summary>
        void DIRdivide(int index)
        {
            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 1. Determine which sides are the largest
            int[] li = new int[base.n];
            for (int i = 0; i < li.Length; i++)
            {
                li[i] = lengths[i][index];
            }
            int biggy = li.Min();
            List<int> ls = new List<int>();
            for (int i = 0; i < li.Length; i++)
            {
                if (li[i] == biggy) ls.Add(i);
            }
            int lssize = ls.Count;
            //////////////////////////////////////////////////////////////////////////////////////



            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 2. Evaluate function in directions of biggest size
            //%--    to determine which direction to make divisions
            double[] oldc = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                oldc[i] = c[i][index];
            }

            double delta = thirds[biggy];


            double[][] newc_left = new double[lssize][];
            double[][] newc_right = new double[lssize][];
            for (int i = 0; i < lssize; i++)
            {
                newc_left[i] = new double[base.n];
                newc_right[i] = new double[base.n];
            }

            for (int i = 0; i < base.n; i++)
            {
                for (int u = 0; u < lssize; u++)
                {
                    newc_left[u][i] = oldc[i];
                    newc_right[u][i] = oldc[i];
                }
            }



            double[] f_left = new double[lssize + 1];         
            double[] f_right = new double[lssize + 1];    


            int oldcounter = base.evalcount;
            for (int i = 0; i < lssize; i++)
            {
                int lsi = ls[i];
                newc_left[i][lsi] = newc_left[i][lsi] - delta;
                newc_right[i][lsi] = newc_right[i][lsi] + delta;
                f_left[i] = base.evalfnc(ReverseNormalization(newc_left[i]));
                Console.WriteLine("fx,x1,x2,{0},{1},{2}", f_left[i], ReverseNormalization(newc_left[i])[0], ReverseNormalization(newc_left[i])[1]);
                if (this.CheckIfNaN(f_left[i])) 
                {
                    this.fxAbort = double.NaN;
                    return; 
                }
                f_right[i] = base.evalfnc(ReverseNormalization(newc_right[i]));   
                Console.WriteLine("fx,x1,x2,{0},{1},{2}", f_right[i], ReverseNormalization(newc_right[i])[0], ReverseNormalization(newc_right[i])[1]);
                if (this.CheckIfNaN(f_right[i]))
                {
                    this.fxAbort = double.NaN;
                    return;
                }
                base.evalcount += 2;                                  
            }
            //////////////////////////////////////////////////////////////////////////////////////
            

            DIRsort(f_left, f_right, ls.ToArray(), oldcounter, index, newc_left, newc_right);
        }


        void DIRsort(double[] f_left, double[] f_right, int[] ls, int oldcounter, int index, double[][] newc_left, double[][] newc_right)
        {
            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 3. Sort w for division order
            double[,] w = new double[ls.Length, 2];
            for (int i = 0; i < ls.Length; i++)
            {
                w[i, 1] = ls[i];
                double min_f;
                if (f_left[i] < f_right[i])
                {
                    min_f = f_left[i];
                }
                else
                {
                    min_f = f_right[i];
                }
                w[i, 0] = min_f;
            }

            double[,] V = new double[ls.Length, 2];
            int[,] order = new int[ls.Length, 2];

            int[] order1 = new int[ls.Length];
            int[] order2 = new int[ls.Length];
            double[] dim1_f = new double[ls.Length];
            int[] dim2_ls = new int[ls.Length];
            for (int i = 0; i < ls.Length; i++)
            {
                dim1_f[i] = w[i, 0];
                dim2_ls[i] = (int)w[i, 1];
                order1[i] = i;
                order2[i] = i;
            }
            Array.Sort(dim1_f, order1);
            Array.Sort(dim2_ls, order2);

            for (int i = 0; i < ls.Length; i++)
            {
                V[i, 0] = dim1_f[i];
                V[i, 1] = dim2_ls[i];
                order[i, 0] = order1[i];
                order[i, 1] = order2[i];
            }
            //////////////////////////////////////////////////////////////////////////////////////




            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 4. Make divisions in order specified by order
            for (int i = 0; i < order.GetLength(0); i++)
            {
                int newleftindex = oldcounter + 2 * i;
                int newrightindex = oldcounter + 2 * i + 1;

                //   %-- 4.1 create new rectangles identical to the old one
                int[] oldrect = new int[base.n];
                for (int u = 0; u < base.n; u++)
                {
                    int copy = lengths[u][index];
                    oldrect[u] = copy;
                }

                for (int u = 0; u < base.n; u++)
                {
                    lengths[u].Add(0);
                    lengths[u].Add(0);
                    int copy = oldrect[u];
                    lengths[u][newleftindex] = copy;
                    lengths[u][newrightindex] = copy;
                }

                //   %-- old, and new rectangles have been sliced in order(i) direction
                lengths[ls[order[i, 1]]][newleftindex] = lengths[ls[order[i, 1]]][index] + 1;
                lengths[ls[order[i, 1]]][newrightindex] = lengths[ls[order[i, 1]]][index] + 1;
                lengths[ls[order[i, 1]]][index] = lengths[ls[order[i, 1]]][index] + 1;

                //   %-- add new columns to c
                for (int u = 0; u < base.n; u++)
                {
                    c[u].Add(0);
                    c[u].Add(0);
                    double copyleft = newc_left[order[i, 0]][u];
                    double copyright = newc_right[order[i, 0]][u];
                    c[u][newleftindex] = copyleft;
                    c[u][newrightindex] = copyright;
                }

                //   %-- add new values to fc
                fc.Add(f_left[order[i, 0]]);
                fc.Add(f_right[order[i, 0]]);

                //   %-- add new values to con
                //   %-- add new flag values to feas_flags
                //   %-- store sizes of each rectangle
                double[] xleft = new double[base.n];
                double[] xright = new double[base.n];
                for (int u = 0; u < base.n; u++)
                {
                    xleft[u] = Math.Pow((double)1 / 3, lengths[u][newleftindex]);
                    xright[u] = Math.Pow((double)1 / 3, lengths[u][newrightindex]);
                }
                double szesleft = 0.5 * Misc.Vector.Norm(xleft);
                double szesright = 0.5 * Misc.Vector.Norm(xright);
                szes.Add(szesleft);
                szes.Add(szesright);
            }


            double[] xindex = new double[base.n];
            for (int u = 0; u < base.n; u++)
            {
                xindex[u] = Math.Pow((double)1 / 3, lengths[u][index]);
            }
            double szesindex = 0.5 * Misc.Vector.Norm(xindex);
            szes[index] = szesindex;
            //////////////////////////////////////////////////////////////////////////////////////
        }



        /// <summary>
        /// Initialization of Direct to eliminate storing floating numbers???...
        /// </summary>
        public void DIRini()
        {
            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //1. thirds
            //%-- start by calculating the thirds array
            //%-- here we precalculate (1/3)^i which we will use frequently
            thirds[0] = (double)1 / 3;
            for (int i = 1; i < 100; i++)
            {
                thirds[i] = (1.0 / 3.0) * thirds[i - 1];
            }
            //////////////////////////////////////////////////////////////////////////////////////


            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //2. lengths
            //%-- length array will store # of slices in each dimension for        
            //%-- each rectangle. dimension will be rows; each rectangle
            //%-- will be a column.
            //             ...one slice is 2 cuts (making a box into 3 boxes)
            //%-- first rectangle is the whole unit hyperrectangle
           
            //4. c, center coordinates
            //%-- first element of c is the center of the unit hyperrectangle
            lengths = new List<int>[base.n];
            c = new List<double>[base.n];
            for (int i = 0; i < base.n; i++)
            {
                lengths[i] = new List<int>();
                lengths[i].Add(0);

                c[i] = new List<double>();
                c[i].Add((double)1 / 2);
            }
            //////////////////////////////////////////////////////////////////////////////////////


            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //3. Size of hyperrectangles
            //%-- store size of hyperrectangle in vector szes
            szes.Add(1);
            //////////////////////////////////////////////////////////////////////////////////////






           

            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //6. Eval function
            //%-- first element of f is going to be the function evaluated
            //%-- at the center of the unit hyper-rectangle.
            double[] xIn = new double[base.n];
            for (int i = 0; i < base.n; i++)
            {
                xIn[i] = c[i][0];
            }
            double fx = base.evalfnc(ReverseNormalization(xIn));
            if (this.CheckIfNaN(fx))
            {
                this.fxAbort = double.NaN;
                return;
            }
            fc.Add(fx);
            Console.WriteLine("fx,x1,x2,{0},{1},{2}", fx, ReverseNormalization(xIn)[0], ReverseNormalization(xIn)[1]);
            base.evalcount++;
            //////////////////////////////////////////////////////////////////////////////////////




            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //7. minval and xatmin
            //  that is: best so far, obj fnuc val and its x
            //            %-- initialize minval and xatmin to be center of hyper-rectangle
            xatmin = new double[base.n];
            Array.Copy(xIn, xatmin, base.n);
            minval = fc[0];

            if (par_testflag == true)
            {
                if (par_globalmin != 0)
                {
                    perror = 100 * (minval - par_globalmin) / Math.Abs(par_globalmin);
                }
                else
                {
                    perror = 100 * minval;
                }
            }
            else
            {
                perror = 2;
            }
            //////////////////////////////////////////////////////////////////////////////////////



            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //8. history
            //%-- initialize history
            history.Add(Tuple.Create(0, base.evalcount, minval));
            //////////////////////////////////////////////////////////////////////////////////////
            
        }


        /// <summary>
        /// Return list of PO hyperrectangles
        /// </summary>
        void find_po()
        {
            //function rects = find_po(fc,lengths,minval,ep,szes)


            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //            %-- 1. Find all rects on hub
            int[] diff_szes = new int[lengths[0].Count];

            for (int u = 0; u < diff_szes.Length; u++)
            {
                diff_szes[u] = 0;
                for (int i = 0; i < base.n; i++)
                {
                    diff_szes[u] = diff_szes[u] + lengths[i][u];
                }
            }

            int tmp_max = diff_szes.Max();

            int j = 0;

            int[] sum_lengths = new int[diff_szes.Length];
            Array.Copy(diff_szes, sum_lengths, diff_szes.Length);

            List<int> hull = new List<int>();

            for (int i = 0; i <= tmp_max; i++)
            {
                List<int> tmp_idx = new List<int>();
                for (int u = 0; u < sum_lengths.Length; u++)
                {
                    if (sum_lengths[u] == i) tmp_idx.Add(u);
                }

                if (tmp_idx.Count > 0)
                {
                    double tmp_n = 0;
                    int hullidx = 0;
                    for (int u = 0; u < tmp_idx.Count; u++)
                    {
                        if (u == 0)
                        {
                            tmp_n = fc[tmp_idx[u]];
                            hullidx = tmp_idx[u];
                        }
                        else if (fc[tmp_idx[u]] < tmp_n)
                        {
                            tmp_n = fc[tmp_idx[u]];
                            hullidx = tmp_idx[u];
                        }
                    }

                    hull.Add(hullidx);
                    j++;

                    //        %-- 1.5 Check for ties
                    List<int> ties = new List<int>();
                    for (int u = 0; u < tmp_idx.Count; u++)
                    {
                        if (Math.Abs(fc[tmp_idx[u]] - tmp_n) <= 1e-13) ties.Add(u);
                    }

                    if (ties.Count > 1)
                    {
                        List<int> mod_ties = new List<int>();
                        for (int u = 0; u < ties.Count; u++)
                        {
                            if (tmp_idx[ties[u]] != hull[j - 1]) mod_ties.Add(u);
                        }

                        for (int u = 0; u < mod_ties.Count; u++)
                        {
                            hull.Add(tmp_idx[ties[mod_ties[u]]]);
                            j++;
                        }
                     }
                 }
            }
            //////////////////////////////////////////////////////////////////////////////////////



            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //          %-- 2. Compute lb and ub for rects on hub
            double[] lbound = calc_lbound(hull);
            double[] ubound = calc_ubound(hull);
            //////////////////////////////////////////////////////////////////////////////////////


            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 3. Find indeces of hull who satisfy
            //%--    1st condition
            List<int> maybe_po = new List<int>();
            for (int i = 0; i < lbound.Length; i++)
            {
                if (lbound[i] - ubound[i] <= 0) maybe_po.Add(i);
            }
            //////////////////////////////////////////////////////////////////////////////////////




            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 4. Find indeces of hull who satisfy
            //%--    2nd condition
            int t_len = maybe_po.Count;

            List<int> po = new List<int>();
            if (minval != 0)
            {
                for (int i = 0; i < t_len; i++)
                {
                    if (((minval - fc[hull[maybe_po[i]]]) / Math.Abs(minval) +
                        szes[hull[maybe_po[i]]] * ubound[maybe_po[i]] / Math.Abs(minval))
                        >= par_ep)
                    {
                        po.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < t_len; i++)
                {
                    if ((fc[hull[maybe_po[i]]] - szes[hull[maybe_po[i]]] * ubound[maybe_po[i]])
                       <= 0)
                    {
                        po.Add(i);
                    }
                }
            }
            
            List<int> final_pos = new List<int>();
            for (int i = 0; i < po.Count; i++)
            {
                final_pos.Add(hull[maybe_po[po[i]]]);
            }
            //////////////////////////////////////////////////////////////////////////////////////





            //____________________________________________________________________________________
            //////////////////////////////////////////////////////////////////////////////////////
            //%-- 6. Return dictionary with potential boxes
            for (int i = 0; i < final_pos.Count; i++)
            {
                S.Add(final_pos[i], szes[final_pos[i]]);
            }
            //////////////////////////////////////////////////////////////////////////////////////
        }




        /// <summary>
        /// Calculate the lbound used in determing potentially optimal hrectangles.
        /// </summary>
        double[] calc_lbound(List<int> hull)
        {
            int hull_length = hull.Count;
            double[] lbDir = new double[hull_length];

            int[,] hull_lengths = new int[lengths.Length, hull_length];
            for (int i = 0; i < lengths.Length; i++)
            {
                for (int u = 0; u < hull_length; u++)
                {
                    hull_lengths[i, u] = lengths[i][hull[u]];
                }
            }


            for (int i = 0; i < hull_length; i++)
            {
                List<int> tmp_rects = new List<int>();
                int[] sum_hull_lengths = new int[hull_lengths.GetLength(1)];
                for (int u = 0; u < hull_lengths.GetLength(1); u++)
                {
                    sum_hull_lengths[u] = 0;
                    for (int k = 0; k < hull_lengths.GetLength(0); k++)
                    {
                        sum_hull_lengths[u] += hull_lengths[k, u];
                    }
                }
                int sum_lengths = 0;
                for (int k = 0; k < lengths.Length; k++)
                {
                    sum_lengths += lengths[k][hull[i]];
                }

                for (int u = 0; u < sum_hull_lengths.Length; u++)
                {
                    if (sum_hull_lengths[u] > sum_lengths)
                    {
                        tmp_rects.Add(u);
                    }
                }


                if (tmp_rects.Count > 0)
                {
                    List<double> tmp_f = new List<double>();
                    List<double> tmp_szes = new List<double>();
                    List<double> tmp_lbs = new List<double>();
                    for (int u = 0; u < tmp_rects.Count; u++)
                    {
                        tmp_f.Add(fc[hull[tmp_rects[u]]]);
                        tmp_szes.Add(szes[hull[tmp_rects[u]]]);
                        tmp_lbs.Add((fc[hull[i]] - tmp_f[u]) / (szes[hull[i]] - tmp_szes[u]));
                    }
                    lbDir[i] = tmp_lbs.Max();
                }
                else
                {
                    lbDir[i] = -1.976e14;
                }

            }
            return lbDir;
        }


        /// <summary>
        /// Calculate the ubound used in determing potentially optimal hrectangles.
        /// </summary>
        double[] calc_ubound(List<int> hull)
        {
            int hull_length = hull.Count;
            double[] ubDir = new double[hull_length];

            int[,] hull_lengths = new int[lengths.Length, hull_length];
            for (int i = 0; i < lengths.Length; i++)
            {
                for (int u = 0; u < hull_length; u++)
                {
                    hull_lengths[i, u] = lengths[i][hull[u]];
                }
            }

            for (int i = 0; i < hull_length; i++)
            {
                List<int> tmp_rects = new List<int>();
                int[] sum_hull_lengths = new int[hull_lengths.GetLength(1)];
                for (int u = 0; u < hull_lengths.GetLength(1); u++)
                {
                    sum_hull_lengths[u] = 0;
                    for (int k = 0; k < hull_lengths.GetLength(0); k++)
                    {
                        sum_hull_lengths[u] += hull_lengths[k, u];
                    }
                }
                int sum_lengths = 0;
                for (int k = 0; k < lengths.Length; k++)
                {
                    sum_lengths += lengths[k][hull[i]];
                }

                for (int u = 0; u < sum_hull_lengths.Length; u++)
                {
                    if (sum_hull_lengths[u] < sum_lengths)
                    {
                        tmp_rects.Add(u);
                    }
                }


                if (tmp_rects.Count > 0)
                {
                    List<double> tmp_f = new List<double>();
                    List<double> tmp_szes = new List<double>();
                    List<double> tmp_ubs = new List<double>();
                    for (int u = 0; u < tmp_rects.Count; u++)
                    {
                        tmp_f.Add(fc[hull[tmp_rects[u]]]);
                        tmp_szes.Add(szes[hull[tmp_rects[u]]]);
                        tmp_ubs.Add((tmp_f[u] - fc[hull[i]]) / (tmp_szes[u] - szes[hull[i]]));
                    }
                    ubDir[i] = tmp_ubs.Min();
                }
                else
                {
                    ubDir[i] = 1.976e14;
                }

            }
            return ubDir;
        }




        private double[] NormalizeX(double[] _x)
        {
            double[] _xNorm = new double[_x.Length];
            for (int i = 0; i < base.n; i++)
            {
                _xNorm[i] = (_x[i] - base.lb[i]) / Math.Abs(base.ub[i] - base.lb[i]);
            }
            return _xNorm;
        }

        private double[] ReverseNormalization(double[] _xNorm)
        {
            double[] _x = new double[_xNorm.Length];
            for (int i = 0; i < _xNorm.Length; i++)
            {
                _x[i] = Math.Abs(base.ub[i] - base.lb[i]) * _xNorm[i] + base.lb[i];
            }
            return _x;
        }
    }


    /*
    /// <summary>
    /// 
    /// Glasmachers et al (2010). Exponential Natural Evolution Strategies. GECCO
    /// </summary>
    public class xNES : SO_Solver
    {
        //function [xopt,fopt] = xnes(f,d,x,timeout)

        //% Written by Sun Yi (yi@idsia.ch).

        //% parameters
        //L = 4+3*floor(log(d));
        //etax = 1; etaA = 0.5*min(1.0/d,0.25);
        //shape = max(0.0, log(L/2+1.0)-log(1:L)); shape = shape / sum(shape);

        //% initialize
        //xopt = x; fopt = f(x);
        //A = zeros(d);
        //weights = zeros(1,L);
        //fit = zeros(1,L);
        //tm = cputime;

        //while cputime - tm < timeout
        //    expA = expm(A);

        //    % step 1: sampling & importance mixing
        //    Z = randn(d,L); X = repmat(x,1,L)+expA*Z;
        //    for i = 1 : L, fit(i) = f(X(:,i)); end

        //    % step 2: fitness reshaping
        //    [~, idx] = sort(fit); weights(idx) = shape;
        //    if fit(idx(1)) < fopt
        //        xopt = X(:,idx(1)); fopt = fit(idx(1));
        //    end

        //    % step 3: compute the gradient for C and x
        //    G = (repmat(weights,d,1).*Z)*Z' - sum(weights)*eye(d);
        //    dx = etax * expA * (Z*weights');
        //    dA = etaA * G;

        //    % step 4: compute the update  
        //    x = x + dx; A = A + dA;

        //    if trace(A)/d < -10*log(10), break; end
        //end

        public xNES(double[] lb, double[] ub, bool[] xint, int itermax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[][] x0 = null)
            : base(lb, ub, xint, itermax, evalfnc, seed)
        {

        }


        public override void solve()
        {
            throw new NotImplementedException();
        }

        protected override void storeCurrentBest()
        {
            throw new NotImplementedException();
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            throw new NotImplementedException();
        }
    }


    public class sNES : SO_Solver
    {

        //        function[mean_vec, var_vec, mean_vec_fit] = snes(fitness, num_iter, dim, pop_size, learn_rates)
        //%[mean_vec, var_vec, mean_vec_fit] = 
        //%           snes(dim, mean_vec, var_vec, pop_size, learn_rates, @fitness)
        //%INPUTS:
        //% @fitness: fitness function (e.g., @rosenfit for rosenfit.m)
        //%           which takes an individual as input and outputs the fitness
        //% num_iter: number of iterations
        //% dim: dimension
        //% **pop_size: number of units to evaluate 
        //% **learn_rates: vector of two, for mean and variance
        //% defaults if pop_size and learn_rates not supplied
        //%OUTPUTS
        //% mean_vec: final mean
        //% var_vec: final variance
        //% mean_vec_fit: the fitness of the mean, measured at each iteration
        //%
        //%code by Matt Luciw (matt.luciw at gmail)
        //%contact Tom Schaul with all your SNES questions

        //%initial mean and variance
        //mean_vec = rand(dim,1);
        //var_vec = ones(dim,1);  %ones!  <-- this is important

        //if (nargin < 4)
        //    %abra cadabra
        //    pop_size = 4 + floor(3 * log(dim));

        //    learn_rates = [1 (3 + log(dim))/(5 * sqrt(dim))];
        //end

        //mean_vec_fit = zeros(1,num_iter);

        //%outer loop: number of population evaluations
        //for i = 1:num_iter

        //    if (mod(i,500)==0) 
        //        fprintf(1, '\nGeneration %d...', i)
        //    end

        //    %draw from standard normal distribution
        //    curr_samples = randn(pop_size,dim);

        //    %add the input mean and variance
        //    curr_members = (curr_samples .* repmat(var_vec',pop_size,1)) + ...
        //        repmat(mean_vec',pop_size,1);

        //    %store samples
        //    S = curr_samples';

        //    %inner loop: number of population members
        //    for j = 1 : pop_size

        //        %fitness evaluated here for this sample (and stored)
        //        fit(j) = fitness(curr_members(j,:));

        //    end

        //    %sort by fitness so most fit guys are last
        //    [dummy order] = sort(fit);

        //    %ordered set of samples
        //    S = S(:,order);

        //    %utilities which must sum to one
        //    %first half of the population has zero utility
        //    threshold = floor(pop_size / 2);
        //    step_size = 1 / threshold; 
        //    U = zeros(1,pop_size);
        //    U(end-threshold+1:end) = step_size:step_size:1;
        //    U = U ./ sum(U);

        //    %compute gradients
        //    %one for mean
        //    mean_grad = U*S';

        //    %variance gradient
        //    S_sq_minus = S.^2 - 1;
        //    var_grad = U * S_sq_minus';

        //    %update parameters
        //    mean_vec = mean_vec + learn_rates(1) * var_vec .* mean_grad';

        //    var_vec = var_vec .* exp(learn_rates(2) / 2 * var_grad)';

        //    %evaluate fitness of mean (for plotting)
        //    mean_vec_fit(i) = fitness(mean_vec);

        //    %uncomment for spiffy updating plot
        //    %plot(mean_vec_fit)
        //    %drawnow
        //end


        public sNES(double[] lb, double[] ub, bool[] xint, int itermax, Func<double[], double> evalfnc, int seed, Dictionary<string, object> settings, double[][] x0 = null)
            : base(lb, ub, xint, itermax, evalfnc, seed)
        {

        }



        public override void solve()
        {
            throw new NotImplementedException();
        }

        protected override void storeCurrentBest()
        {
            throw new NotImplementedException();
        }

        protected override bool CheckIfNaN(double fxtest)
        {
            throw new NotImplementedException();
        }
    }



    
    /// <summary>
    /// options: deterministic or stochastic (metropolis) acceptance rules
    ///  Moscato & Fontanari 1990
    /// implement re-annealing, with x* of previous run as new x0
    /// </summary>
    public class SimpleSA : SO_Solver
    {

        public override void solve()
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// Differential Evolution.
    /// Storn and Price (1997). Differential Evolution - A Simple and Efficient Heuristic for Global Optimization over Contiuous Spaces.
    /// </summary>
    public class SimpleDE : SO_Solver
    {

        public override void solve()
        {
            throw new NotImplementedException();
        }
    }





    /// <summary>
    /// 
    /// </summary>
    public class Rosenbrock : SO_Solver
    {

        public override void solve()
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// with probabilistic restart...
    /// Luersen & Le Riche (2004). Globalized Nelder-Mead method for engineering optimization.
    /// </summary>
    public class NelderMead : SO_Solver
    {

        public override void solve()
        {
            throw new NotImplementedException();
        }
    }
    */
}
