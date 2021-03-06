﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaheuristicsTuner.Testfunctions;
using MetaheuristicsLibrary.SolversSO;
using MetaheuristicRepository.Solvers_SO;
using MetaheuristicRepository;
using System.IO;

using ILOG.CPLEX;
using ILOG.Concert;


namespace MetaheuristicsTuner
{
    class Program
    {

        static void Main(string[] args)
        {
            //______________________________________________________________________________________
            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////

            //SGA PARAMETERS
            double[] hp = new double[7];
            hp[0] = 15.20115;   // popsize
            hp[1] = 90.24627;   // k
            hp[2] = 1;  // pcross
            hp[3] = 0.80054;    // pmut
            hp[4] = 1.80471;    // d
            hp[5] = 0.01;   // r
            hp[6] = 0.26437;	// elites


            ////ES parameters
            //double[] hp = new double[8];
            //// cluster a
            //hp[0] = 2;
            //hp[1] = 1;
            //hp[2] = 0.16085;
            //hp[3] = 0.07627;
            //hp[4] = 0.01;
            //hp[5] = 0.2288;
            //hp[6] = 0.01136;
            //hp[7] = 0.92639;


            ////PSO PARAmeters
            //double[] hp = new double[7];
            //// cluster  
            //hp[0] = 9.03316;
            //hp[1] = 0.001;
            //hp[2] = 18.24318;
            //hp[3] = 0;
            //hp[4] = 1;
            //hp[5] = 0;
            //hp[6] = 3.22193;



            ////FIPSO PARAMeters
            //double[] hp = new double[5];
            //// cluster a
            //hp[0] = 12.16184;
            //hp[1] = 0.55492;
            //hp[2] = 9.31087;
            //hp[3] = 0.59655;
            //hp[4] = 1;

            TestHyperParam("SGA", hp, 13, 100, 30);

            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////






            //______________________________________________________________________________________
            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////

            //IdentifyTwoBestParameterSets("FIPS");    //"SGA", "ES", "PSO", "FIPS"

            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////







            //______________________________________________________________________________________
            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////

            //TuneSolver("SGA", 20, 100);

            ////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////
        }




        /// <summary>
        /// Getting maximum congruency for naming parameter sets A or B. Linear Programming optimization model.
        /// </summary>
        static void IdentifyTwoBestParameterSets(string caseSolver)
        {
            Cplex cpl = new Cplex();

            int N = 7;
            //string caseSolver = "ES"; //"SGA" (default), "ES", "PSO", "FIPS" 

            int[] ctopA;
            int[] cTopB;
            int[] cBtmA;
            int[] cBtmB;
            switch (caseSolver)
            {
                default:
                    ctopA = new int[] { 11, 1, 1, 13, 0, 0, 2 };
                    cTopB = new int[] { 0, 5, 3, 0, 5, 5, 5 };
                    cBtmA = new int[] { 2, 12, 12, 0, 14, 14, 9 };
                    cBtmB = new int[] { 4, 0, 1, 4, 0, 0, 0 };
                    break;
                case "ES":
                    ctopA = new int[] { 4, 6, 9, 3, 11, 11, 5 };
                    cTopB = new int[] { 2, 3, 1, 3, 1, 3, 3 };
                    cBtmA = new int[] { 4, 5, 2, 8, 0, 0, 6 };
                    cBtmB = new int[] { 3, 1, 3, 2, 5, 3, 2 };
                    break;
                case "PSO":
                    ctopA = new int[] { 1, 3, 2, 3, 3, 7, 5 };
                    cTopB = new int[] { 12, 11, 3, 1, 7, 8, 5 };
                    cBtmA = new int[] { 7, 5, 6, 5, 5, 1, 3 };
                    cBtmB = new int[] { 0, 1, 9, 11, 5, 4, 7 };
                    break;
                case "FIPS":
                    ctopA = new int[] { 6, 6, 7, 3, 5, 0, 8 };
                    cTopB = new int[] { 4, 6, 6, 9, 5, 9, 1 };
                    cBtmA = new int[] { 4, 4, 3, 7, 5, 10, 2 };
                    cBtmB = new int[] { 6, 4, 4, 1, 5, 1, 9 };
                    break;
            }


            INumVar[] xTopA = new INumVar[N];
            INumVar[] xBtmB = new INumVar[N];
            INumVar[] xTopB = new INumVar[N];
            INumVar[] xBtmA = new INumVar[N];
            ILinearNumExpr value = cpl.LinearNumExpr();
            for (int n = 0; n < N; n++)
            {
                xTopA[n] = cpl.BoolVar();
                xBtmB[n] = cpl.BoolVar();
                xTopB[n] = cpl.BoolVar();
                xBtmA[n] = cpl.BoolVar();
                cpl.AddEq(cpl.Sum(xTopB[n], xTopA[n]), 1);
                cpl.AddEq(cpl.Sum(xBtmA[n], xBtmB[n]), 1);
                cpl.AddEq(cpl.Sum(xTopA[n], xBtmA[n]), 1);
                cpl.AddEq(cpl.Sum(xTopB[n], xBtmB[n]), 1);
                value.AddTerm(xTopA[n], ctopA[n]);
                value.AddTerm(xTopB[n], cTopB[n]);
                value.AddTerm(xBtmA[n], cBtmA[n]);
                value.AddTerm(xBtmB[n], cBtmB[n]);
            }

            cpl.AddMaximize(value);
            cpl.Solve();

            Console.WriteLine("Parameter Grouping for Solver: {0}", caseSolver);
            for (int n = 0; n < N; n++)
            {
                Console.WriteLine("n: {0}", n);
                Console.WriteLine("xtopA: ;{0};, _____, xTopB: ;{1};", cpl.GetValue(xTopA[n]), cpl.GetValue(xTopB[n]));
                Console.WriteLine("xbtmB: ;{0};, _____, xBtmA: ;{1};", cpl.GetValue(xBtmB[n]), cpl.GetValue(xBtmA[n]));
            }
            Console.WriteLine("cost: {0}", cpl.GetObjValue());
            Console.ReadKey();




        }

        
        /// <summary>
        /// Testing a hyper-parameter configuration for one test function
        /// </summary>
        /// <param name="args"></param>
        static void TestHyperParamOneFuncOnly()
        {
            string solver = "SGA";
            //double[] hp;
            int testfuncIndex = 19;
            int testfuncdim = 35;
            int evalbudgetmultipl = 35;
            int rerunsTestFunc = 35;

            //SGA PARAMETERS
            double[] hp = new double[7];
            hp[0] = 15.20115;   // popsize
            hp[1] = 90.24627;   // k
            hp[2] = 1;  // pcross
            hp[3] = 0.80054;    // pmut
            hp[4] = 1.80471;    // d
            hp[5] = 0.01;   // r
            hp[6] = 0.26437;	// elites


            ////ES parameters
            //double[] hp = new double[8];
            //// cluster a
            //hp[0] = 2;
            //hp[1] = 1;
            //hp[2] = 0.16085;
            //hp[3] = 0.07627;
            //hp[4] = 0.01;
            //hp[5] = 0.2288;
            //hp[6] = 0.01136;
            //hp[7] = 0.92639;


            ////PSO PARAmeters
            //double[] hp = new double[7];
            //// cluster  
            //hp[0] = 9.03316;
            //hp[1] = 0.001;
            //hp[2] = 18.24318;
            //hp[3] = 0;
            //hp[4] = 1;
            //hp[5] = 0;
            //hp[6] = 3.22193;



            ////FIPSO PARAMeters
            //double[] hp = new double[5];
            //// cluster a
            //hp[0] = 12.16184;
            //hp[1] = 0.55492;
            //hp[2] = 9.31087;
            //hp[3] = 0.59655;
            //hp[4] = 1;


            //TestHyperParamOneFuncOnly("FIPSO", hp, 19, 35, 30, 30);





            //int evalbudgetmultiplier = 100;
            //int testfuncdim = 20;                //tuned to this n
            //int rerunsTestFuncs = 30;            // 30
            HyperFuncs hf = new HyperFuncs(testfuncdim, rerunsTestFunc, solver, evalbudgetmultipl);   //problem dim; reruns of testfuncs; solver to be tuned
            Func<double[], double[]>[] manyhyperfuncs = new Func<double[], double[]>[20];
            manyhyperfuncs[0] = hf.HyperFunc_B_Perm0db_dist;
            manyhyperfuncs[1] = hf.HyperFunc_B_RotHypEll_dist;
            manyhyperfuncs[2] = hf.HyperFunc_B_Sphere_dist;
            manyhyperfuncs[3] = hf.HyperFunc_B_SumSquares_dist;
            manyhyperfuncs[4] = hf.HyperFunc_B_Trid_dist;
            manyhyperfuncs[5] = hf.HyperFunc_L_Ackley_dist;
            manyhyperfuncs[6] = hf.HyperFunc_L_Griewank_dist;
            manyhyperfuncs[7] = hf.HyperFunc_L_Levy_dist;
            manyhyperfuncs[8] = hf.HyperFunc_L_Rastrigin_dist;
            manyhyperfuncs[9] = hf.HyperFunc_L_Schwefel_dist;
            manyhyperfuncs[10] = hf.HyperFunc_O_PermDB_dist;
            manyhyperfuncs[11] = hf.HyperFunc_O_StyblinskiTang_dist;
            manyhyperfuncs[12] = hf.HyperFunc_P_Zhakarov_dist;
            manyhyperfuncs[13] = hf.HyperFunc_V_DixonPrice_dist;
            manyhyperfuncs[14] = hf.HyperFunc_V_Rosenbrock_dist;
            manyhyperfuncs[15] = hf.HyperFunc_L_Levy_Edge_dist;
            manyhyperfuncs[16] = hf.HyperFunc_L_Schwefel_Edge_dist;
            manyhyperfuncs[17] = hf.HyperFunc_O_StyblinskiTang_Edge_dist;
            manyhyperfuncs[18] = hf.HyperFunc_V_DixonPrice_Edge_dist;
            manyhyperfuncs[19] = hf.HyperFunc_V_Rosenbrock_Edge_dist;

            double [] dist = manyhyperfuncs[testfuncIndex](hp);
            foreach (double fx in dist)
            {
                Console.WriteLine(fx);
            }
            Console.WriteLine();
            Console.WriteLine("expected value: {0}", dist.Average());
            Console.ReadKey();


        }

        /// <summary>
        /// Testing a hyper-parameter configuration for all test functions
        /// </summary>
        /// <param name="args"></param>
        static void TestHyperParam(string solver, double [] hp, int testfuncdim, int evalbudgetmultipl, int rerunsTestFuncs)
        {
            // solver = "SGA";
            ////double[] hp;
            // testfuncdim = 13;
            //int evalbudgetmultipl = 100;
            //int rerunsTestFuncs = 30;


            ////SGA PARAMETERS
            //double[] hp = new double[7];
            //hp[0] = 15.20115;   // popsize
            //hp[1] = 90.24627;   // k
            //hp[2] = 1;  // pcross
            //hp[3] = 0.80054;    // pmut
            //hp[4] = 1.80471;    // d
            //hp[5] = 0.01;   // r
            //hp[6] = 0.26437;	// elites


            ////ES parameters
            //double[] hp = new double[8];
            //// cluster a
            //hp[0] = 2;
            //hp[1] = 1;
            //hp[2] = 0.16085;
            //hp[3] = 0.07627;
            //hp[4] = 0.01;
            //hp[5] = 0.2288;
            //hp[6] = 0.01136;
            //hp[7] = 0.92639;


            ////PSO PARAmeters
            //double[] hp = new double[7];
            //// cluster  
            //hp[0] = 9.03316;
            //hp[1] = 0.001;
            //hp[2] = 18.24318;
            //hp[3] = 0;
            //hp[4] = 1;
            //hp[5] = 0;
            //hp[6] = 3.22193;



            ////FIPSO PARAMeters
            //double[] hp = new double[5];
            //// cluster a
            //hp[0] = 12.16184;
            //hp[1] = 0.55492;
            //hp[2] = 9.31087;
            //hp[3] = 0.59655;
            //hp[4] = 1;



            //TestHyperParam("SGA", hp, 13, 100, 30);





            //int evalbudgetmultiplier = 100;
            //int testfuncdim = 20;                //tuned to this n
            //int rerunsTestFuncs = 30;            // 30
            HyperFuncs hf = new HyperFuncs(testfuncdim, rerunsTestFuncs, solver, evalbudgetmultipl);   //problem dim; reruns of testfuncs; solver to be tuned
            Func<double[], double>[] manyhyperfuncs = new Func<double[], double>[20];
            manyhyperfuncs[0] = hf.HyperFunc_B_Perm0db;
            manyhyperfuncs[1] = hf.HyperFunc_B_RotHypEll;
            manyhyperfuncs[2] = hf.HyperFunc_B_Sphere;
            manyhyperfuncs[3] = hf.HyperFunc_B_SumSquares;
            manyhyperfuncs[4] = hf.HyperFunc_B_Trid;
            manyhyperfuncs[5] = hf.HyperFunc_L_Ackley;
            manyhyperfuncs[6] = hf.HyperFunc_L_Griewank;
            manyhyperfuncs[7] = hf.HyperFunc_L_Levy;
            manyhyperfuncs[8] = hf.HyperFunc_L_Rastrigin;
            manyhyperfuncs[9] = hf.HyperFunc_L_Schwefel;
            manyhyperfuncs[10] = hf.HyperFunc_O_PermDB;
            manyhyperfuncs[11] = hf.HyperFunc_O_StyblinskiTang;
            manyhyperfuncs[12] = hf.HyperFunc_P_Zhakarov;
            manyhyperfuncs[13] = hf.HyperFunc_V_DixonPrice;
            manyhyperfuncs[14] = hf.HyperFunc_V_Rosenbrock;
            manyhyperfuncs[15] = hf.HyperFunc_L_Levy_Edge;
            manyhyperfuncs[16] = hf.HyperFunc_L_Schwefel_Edge;
            manyhyperfuncs[17] = hf.HyperFunc_O_StyblinskiTang_Edge;
            manyhyperfuncs[18] = hf.HyperFunc_V_DixonPrice_Edge;
            manyhyperfuncs[19] = hf.HyperFunc_V_Rosenbrock_Edge;

            double[] means = new double[manyhyperfuncs.Length];
            for (int i = 0; i < manyhyperfuncs.Length; i++)
            {
                means[i] = manyhyperfuncs[i](hp);
                Console.WriteLine(means[i]);
            }
            Console.ReadKey();


        }


        /// <summary>
        /// Tuning hyper-parameters of a solver
        /// </summary>
        /// <param name="args"></param>
        static void TuneSolver(string solver, int testfuncdim, int evalbudgetmultilp)
        {
            //string solver = "PSO";               //choosing solver to be tuned. string: "SGA", "ES", "FIPSO", "SA"
            int maxfunc = 5000;                  //func calls of the meta-optimizer. 5000
            //int testfuncdim = 20;                //tuned to this n
            int rerunsMeta = 10;                 // 10
            int rerunsTestFuncs = 30;            // 30


            HyperFuncs hf = new HyperFuncs(testfuncdim, rerunsTestFuncs, solver, evalbudgetmultilp);   //problem dim; reruns of testfuncs; solver to be tuned
            int[] seeds_in = new int[rerunsMeta]; //reruns of meta-optimizer
            for (int i = 0; i < seeds_in.Length; i++) seeds_in[i] = i;

            int dvar;
            double[] ub, lb;
            bool[] xint;
            hf.getXandBounds(out dvar, out lb, out ub, out xint, solver);


            Console.WriteLine();
            Console.WriteLine(@"///////////////////////////////////////");
            Console.WriteLine("Hyperparameter optimization of...: {0}", solver);
            Console.WriteLine(@"tuned to n = {0}", testfuncdim);
            Console.WriteLine(@"Meta-optimizers: SGA and ES. Each with {0} re-runs", seeds_in.Length);
            Console.WriteLine(@"///////////////////////////////////////");
            Console.WriteLine();
            Console.WriteLine(@"Please enter an existing output path for results. E.g.: c:\n13\");
            string basepath = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine(@"How many threads would you like to assign to this? Please enter an integer between 1 and 20.");
            int maxthreads = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Thanks! Starting... This might take a couple of days.");





            //Func<double[], double>[] manyhyperfuncs = new Func<double[], double>[1];
            //manyhyperfuncs[0] = hf.HyperFunc_B_Perm0db;
            //manyhyperfuncs[1] = hf.HyperFunc_B_RotHypEll;
            //manyhyperfuncs[2] = hf.HyperFunc_L_Ackley;
            //manyhyperfuncs[3] = hf.HyperFunc_O_PermDB;
            //manyhyperfuncs[4] = hf.HyperFunc_L_Schwefel_Edge;
            //manyhyperfuncs[0] = hf.HyperFunc_L_Levy_Edge;

            Func<double[], double>[] manyhyperfuncs = new Func<double[], double>[20];
            manyhyperfuncs[0] = hf.HyperFunc_B_Perm0db;
            manyhyperfuncs[1] = hf.HyperFunc_B_RotHypEll;
            manyhyperfuncs[2] = hf.HyperFunc_B_Sphere;
            manyhyperfuncs[3] = hf.HyperFunc_B_SumSquares;
            manyhyperfuncs[4] = hf.HyperFunc_B_Trid;
            manyhyperfuncs[5] = hf.HyperFunc_L_Ackley;
            manyhyperfuncs[6] = hf.HyperFunc_L_Griewank;
            manyhyperfuncs[7] = hf.HyperFunc_L_Levy;
            manyhyperfuncs[8] = hf.HyperFunc_L_Rastrigin;
            manyhyperfuncs[9] = hf.HyperFunc_L_Schwefel;
            manyhyperfuncs[10] = hf.HyperFunc_O_PermDB;
            manyhyperfuncs[11] = hf.HyperFunc_O_StyblinskiTang;
            manyhyperfuncs[12] = hf.HyperFunc_P_Zhakarov;
            manyhyperfuncs[13] = hf.HyperFunc_V_DixonPrice;
            manyhyperfuncs[14] = hf.HyperFunc_V_Rosenbrock;
            manyhyperfuncs[15] = hf.HyperFunc_L_Levy_Edge;
            manyhyperfuncs[16] = hf.HyperFunc_L_Schwefel_Edge;
            manyhyperfuncs[17] = hf.HyperFunc_O_StyblinskiTang_Edge;
            manyhyperfuncs[18] = hf.HyperFunc_V_DixonPrice_Edge;
            manyhyperfuncs[19] = hf.HyperFunc_V_Rosenbrock_Edge;




            //manyhyperfuncs[15] = hf.HyperFunc_B_RotHypEll_Edge;
            //manyhyperfuncs[16] = hf.HyperFunc_B_Sphere_Edge;
            //manyhyperfuncs[17] = hf.HyperFunc_B_SumSquares_Edge;
            //manyhyperfuncs[18] = hf.HyperFunc_L_Ackley_Edge;
            //manyhyperfuncs[19] = hf.HyperFunc_L_Griewank_Edge;
            //manyhyperfuncs[21] = hf.HyperFunc_L_Rastrigin_Edge;
            //manyhyperfuncs[24] = hf.HyperFunc_P_Zhakarov_Edge;




            //meta optimizer SGA
            Dictionary<string, object> settingsSGA = new Dictionary<string, object>();
            settingsSGA.Add("maxgen", 10000);
            settingsSGA.Add("popsize", 20);
            settingsSGA.Add("elite", 1);
            settingsSGA.Add("pcross", 1);
            settingsSGA.Add("pmut", 0.3);
            settingsSGA.Add("d", 1.1);
            settingsSGA.Add("r", 0.1);
            settingsSGA.Add("k", 6);

            //meta optimizer SA
            Hybrid.settings settingsSA = HybridAlgorithms.SA(maxfunc);

            //meta optimizer FIPSO
            Dictionary<string, object> settingsPSO = new Dictionary<string, object>();

            //meta optimizer ES
            Dictionary<string, object> settingsES = new Dictionary<string, object>();
            settingsES.Add("popsize", 20);          // ∈ {2,...,200}
            settingsES.Add("lambda", 10);            // ∈ {1,...,200}
            settingsES.Add("roh", 2);               // ∈ {1,...,popsize}  . in hyperoptimization, express as percentage of lambda
            settingsES.Add("x0sampling", 1);        // ∈ {0,1}  0=uniform, 1=gaussian
            settingsES.Add("stepsize0", 5);       // ∈ [0.01, 10]
            settingsES.Add("stepsize", 2.2);        // ∈ [0.01, 10]
            settingsES.Add("tauc", 1);              // ∈ [0.01, 50]


           

            Parallel.For(0, manyhyperfuncs.Length, new ParallelOptions { MaxDegreeOfParallelism = maxthreads }, hh =>
            {
                //for(int hh=0; hh<manyhyperfuncs.Length; hh++){
                Console.WriteLine("started hh {0} of {1}", hh, manyhyperfuncs.Length - 1);
                List<string> log_SA = new List<string>();
                List<string> log_ES = new List<string>();
                List<string> log_PSO = new List<string>();
                List<string> log_SGA = new List<string>();
                string str;
                for (int iseeds = 0; iseeds < seeds_in.Length; iseeds++)
                {
                    /////////////////////////////////////////            SGA          /////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    SimpleGA ga = new SimpleGA(lb, ub, xint, maxfunc, manyhyperfuncs[hh], iseeds, settingsSGA);
                    ga.solve();
                    ga.get_fxoptimum();
                    str = "Last call;" + Math.Round(ga.get_fxoptimum(), 4);
                    for (int xx = 0; xx < dvar; xx++)
                    {
                        str += ";" + Math.Round(ga.get_Xoptimum()[xx], 5);
                    }
                    str += ";SGA";
                    log_SGA.Add(str);
                    Console.WriteLine("Metaoptimizer SGA DONE, seed {0} and hh {1}", iseeds, hh);


                    /////////////////////////////////////////            ES           /////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    SimpleES es = new SimpleES(lb, ub, xint, maxfunc, manyhyperfuncs[hh], iseeds, settingsSGA);
                    es.solve();
                    es.get_fxoptimum();
                    str = "Last call;" + Math.Round(es.get_fxoptimum(), 4);
                    for (int xx = 0; xx < dvar; xx++)
                    {
                        str += ";" + Math.Round(es.get_Xoptimum()[xx], 5);
                    }
                    str += ";ES";
                    log_ES.Add(str);
                    Console.WriteLine("Metaoptimizer ES DONE, seed {0} and hh {1}", iseeds, hh);




                    ///////////////////////////////////////////            FIPSO        /////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //PSO pso = new PSO(lb, ub, xint, maxfunc, manyhyperfuncs[hh], iseeds, settingsPSO);
                    //pso.solve();
                    //pso.get_fxoptimum();
                    //str = "Last call;" + Math.Round(pso.get_fxoptimum(), 4);
                    //for (int xx = 0; xx < dvar; xx++)
                    //{
                    //    str += ";" + Math.Round(pso.get_Xoptimum()[xx], 5);
                    //}
                    //str += ";PSO";
                    //log_PSO.Add(str);
                    //Console.WriteLine("Metaoptimizer PSO DONE, seed {0} and hh {1}", iseeds, hh);



                    ///////////////////////////////////////////            Hybrid.SA          ///////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //ISingleObjOpti hyperopti = new Hybrid(dvar, lb, ub, manyhyperfuncs[hh], iseeds, settingsSA);
                    //hyperopti.initialize();
                    //hyperopti.Solve();
                    //str = "Last call;" + Math.Round(hyperopti.fxBest, 4);
                    //for (int xx = 0; xx < dvar; xx++)
                    //{
                    //    str += ";" + Math.Round(hyperopti.xBest[xx], 5);
                    //}
                    //str += ";SA";
                    //log_SA.Add(str);
                    //Console.WriteLine("Metaoptimizer SA DONE, seed {0} and hh {1}", iseeds, hh);
                }


                /////////////////////////////////////////            Writing          ///////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                string fileName = basepath + solver + @"_hf_" + hh + "_AllSeeds.txt";
                using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < log_SA.Count; i++)
                        sw.WriteLine(log_SA[i]);
                    for (int i = 0; i < log_ES.Count; i++)
                        sw.WriteLine(log_ES[i]);
                    for (int i = 0; i < log_SGA.Count; i++)
                        sw.WriteLine(log_SGA[i]);
                    for (int i = 0; i < log_PSO.Count; i++)
                        sw.WriteLine(log_PSO[i]);
                }



                Console.WriteLine("Done hh {0} of {1}", hh, manyhyperfuncs.Length);
            });
            //}

            //hf.printLogs(basepath, 15);

            Console.WriteLine();
            Console.WriteLine(@"///////////////////////////////////////");
            Console.WriteLine("Done with everything, tuning the {0}", solver);
            Console.WriteLine(@"///////////////////////////////////////");
            Console.ReadKey();

        }

    }


    internal class HyperFuncs
    {
        int testfuncDim;
        int runsperTestfunc;
        string solver;
        int evalbudget;

        List<string> logs = new List<string>();
        public void printLogs(string basepath, int hh)
        {
            string fileName = basepath + solver + @"_hf_" + hh + "_AllEvals.txt";
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach(string l in logs)
                {
                    sw.WriteLine(l);        
                }
            }
        }


        internal HyperFuncs(int dim, int reruns, string solvername, int evaluationBudgetMultiplier)
        {
            testfuncDim = dim;
            runsperTestfunc = reruns;
            solver = solvername;
            evalbudget = evaluationBudgetMultiplier;

        }


        public void getXandBounds(out int dvar, out double[] lb, out double[] ub, out bool[] xint, string solver)
        {
            switch (solver)
            {
                default:    //SGA
                    dvar = 7;
                    lb = new double[dvar];
                    ub = new double[dvar];
                    xint = new bool[dvar];
                    lb[0] = 5;          //simpleGAsettings.Add("popsize", Convert.ToInt32(x[0]));            //x[0]  50
                    ub[0] = 200;
                    xint[0] = false;
                    lb[1] = 1;          //simpleGAsettings.Add("k", Convert.ToInt32(x[1]));//x[1]  6
                    ub[1] = 100;
                    xint[1] = false;
                    lb[2] = 0.01;       //simpleGAsettings.Add("pcross", x[2]);           //x[2]  0.7
                    ub[2] = 1.0;
                    xint[2] = false;
                    lb[3] = 0;       //simpleGAsettings.Add("pmut", x[3]);             //x[3]  0.3
                    ub[3] = 1.0;
                    xint[3] = false;
                    lb[4] = 0.01;       //simpleGAsettings.Add("d", x[4]);                //x[4]  0.1
                    ub[4] = 2.0;
                    xint[4] = false;
                    lb[5] = 0.01;       //simpleGAsettings.Add("r", x[5]);                //x[5]  0.1
                    ub[5] = 2.0;
                    xint[5] = false;
                    lb[6] = 0.0;       //simpleGAsettings.Add("elite", x[6]);                //x[6]  
                    ub[6] = 1.0;
                    xint[6] = false;
                    break;
                case "ES":
                    dvar = 8;
                    lb = new double[dvar];
                    ub = new double[dvar];
                    xint = new bool[dvar];
                    lb[0] = 2;      // "popsize" ∈ {2,...,200}
                    ub[0] = 200;
                    xint[0] = false;
                    lb[1] = 1;      // "lambda" (offspring) ∈ {1,...,200}
                    ub[1] = 200;
                    xint[1] = false;
                    lb[2] = 0;      // "roh" ∈ {1,...,popsize} in %
                    ub[2] = 1.0;
                    xint[2] = false;
                    lb[3] = 0;      // "x0sampling" ∈ {0,1}  0=uniform, 1=gaussian
                    ub[3] = 1.0;
                    xint[3] = false;
                    lb[4] = 0.01;   // "stepsize0" ∈ [0.01, 10]
                    ub[4] = 10;
                    xint[4] = false;
                    lb[5] = 0.01;   // "stepsie" ∈ [0.01, 10]
                    ub[5] = 10;
                    xint[5] = false;
                    lb[6] = 0.01;   // "tauc" ∈ [0.01, 50]
                    ub[6] = 50;
                    xint[6] = false;
                    lb[7] = 0;      // "selmode" ∈ {0,1}
                    ub[7] = 1;
                    xint[7] = false;
                    break;
                case "FIPSO":
                    dvar = 5;
                    lb = new double[dvar];
                    ub = new double[dvar];
                    xint = new bool[dvar];
                    lb[0] = 4;
                    ub[0] = 200;
                    xint[0] = false;        // "popsize" ∈ {4,...,200}
                    lb[1] = 0.001;
                    ub[1] = 1;
                    xint[1] = false;        // "chi" constriction coefficient ∈ [0.001,1]
                    lb[2] = 0;
                    ub[2] = 50;
                    xint[2] = false;        // "phi" attraction to best particles ∈ [0,10]
                    lb[3] = 0;
                    ub[3] = 20;
                    xint[3] = false;        // "v0max" initial velocity multiplicator ∈ [0,20]
                    lb[4] = 0;
                    ub[4] = 1;
                    xint[4] = true;         //0 = update after population. 1 = update after each evaluation 
                    //lb[5] = 0;  
                    //ub[5] = 2;
                    //xint[5] = true;         //0 = fipso, 1 = inertia, 2 = constriction 
                    //lb[6] = 0;
                    //ub[6] = 5;      
                    //xint[6] = false;        //attraction own best ∈ [0,5]
                    //lb[7] = 0;
                    //ub[7] = 5;
                    //xint[7] = false;        //attraction global best ∈ [0,5]
                    break;
                case "PSO":
                    dvar = 7;
                    lb = new double[dvar];
                    ub = new double[dvar];
                    xint = new bool[dvar];
                    lb[0] = 4;
                    ub[0] = 200;
                    xint[0] = false;        // "popsize" ∈ {4,...,200}
                    lb[1] = 0.001;
                    ub[1] = 1;
                    xint[1] = false;        // "chi" constriction coefficient ∈ [0.001,1]
                    //lb[2] = 0;
                    //ub[2] = 50;
                    //xint[2] = false;        // "phi" attraction to best particles ∈ [0,50]
                    lb[2] = 0;
                    ub[2] = 20;
                    xint[2] = false;        // "v0max" initial velocity multiplicator ∈ [0,20]
                    lb[3] = 0;
                    ub[3] = 1;
                    xint[3] = true;         //0 = update after population. 1 = update after each evaluation 
                    lb[4] = 1;
                    ub[4] = 2;
                    xint[4] = true;         //0 = fipso, 1 = inertia, 2 = constriction. fipso deactivated here 
                    lb[5] = 0;
                    ub[5] = 5;
                    xint[5] = false;        //attraction own best ∈ [0,5]
                    lb[6] = 0;
                    ub[6] = 5;
                    xint[6] = false;        //attraction global best ∈ [0,5]
                    break;

                //case "SA":

                //    break;
            }

        }




        //ES has 8 parameters
        private double meanES(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> settingsES = new Dictionary<string, object>();
            int simpleESpop = Convert.ToInt16(hpo_x[0]);
            int simpleESLambda = Convert.ToInt16(hpo_x[1]);
            int simpleESroh = Convert.ToInt16(hpo_x[2] * simpleESLambda) + 1; //from percentage to integer
            settingsES.Add("popsize", simpleESpop);          // ∈ {2,...,200}
            settingsES.Add("lambda", simpleESLambda);            // ∈ {1,...,200}
            settingsES.Add("roh", simpleESroh);               // ∈ {1,...,popsize}  . in hyperoptimization, express as percentage of lambda
            settingsES.Add("x0sampling", Convert.ToInt16(hpo_x[3]));        // ∈ {0,1}  0=uniform, 1=gaussian
            settingsES.Add("stepsize0", hpo_x[4]);       // ∈ [0.01, 10]
            settingsES.Add("stepsize", hpo_x[5]);        // ∈ [0.01, 10]
            settingsES.Add("tauc", hpo_x[6]);              // ∈ [0.01, 10]
            settingsES.Add("selmode", hpo_x[7]);        // 0 = random, 1=roulette wheel
            MetaheuristicsLibrary.SolversSO.SO_Solver[] simpleES = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];

            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                simpleES[i] = new MetaheuristicsLibrary.SolversSO.SimpleES(lb, ub, xint, _maxfuncs, testfunc, i, settingsES);
                simpleES[i].solve();
                mins[i] = simpleES[i].get_fxoptimum();
            }
            return mins.Average();
        }

        //SGA has 7 parameters
        private double meanSGA(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> simpleGAsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] simpleGAs = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int simplegapop = Convert.ToInt32(hpo_x[0]);
            simplegapop += simplegapop % 2;
            simpleGAsettings.Add("popsize", simplegapop);            //x[0]  50
            simpleGAsettings.Add("maxgen", Convert.ToInt32(Math.Floor(_maxfuncs / Convert.ToDouble(simplegapop))));            //no x. its a function of popsize and max evals.  50
            simpleGAsettings.Add("k", hpo_x[1]);                //x[1]  6
            simpleGAsettings.Add("pcross", hpo_x[2]);           //x[2]  0.7
            simpleGAsettings.Add("pmut", hpo_x[3]);             //x[3]  0.3
            simpleGAsettings.Add("d", hpo_x[4]);                //x[4]  0.1
            simpleGAsettings.Add("r", hpo_x[5]);                //x[5]  0.1
            int SGAelite = Convert.ToInt16(hpo_x[6] * (int)Math.Round((double)simplegapop / 2, 0)); //from percentage to integer
            simpleGAsettings.Add("elite", SGAelite);                //x[6]  0 - (popsize/2). 

            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                simpleGAs[i] = new MetaheuristicsLibrary.SolversSO.SimpleGA(lb, ub, xint, _maxfuncs, testfunc, i, simpleGAsettings);
                simpleGAs[i].solve();
                mins[i] = simpleGAs[i].get_fxoptimum();
            }

            return mins.Average();
        }

        private double meanFIPSO(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> PSOsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] PSO = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int fipsopop = Convert.ToInt16(hpo_x[0]);
            PSOsettings.Add("popsize", fipsopop);             //x[0] ∈ {4,..., 100}
            PSOsettings.Add("chi", hpo_x[1]);                 //x[1] ∈ [0.001, 1], constriction coefficient
            PSOsettings.Add("phi", hpo_x[2]);                 //x[2] ∈ [0, 50], attraction to best particle
            PSOsettings.Add("v0max", hpo_x[3]);               //x[3] ∈ [0, 10], initial velocity
            PSOsettings.Add("pxupdatemode", Convert.ToInt16(hpo_x[4])); //x[4] 0 = update after population. 1 = update after each evaluation 
            PSOsettings.Add("psomode", 0);
            //PSOsettings.Add("psomode", Convert.ToInt16(hpo_x[5])); //x[5] 0 = fipso, 1 = inertia, 2 = constriction 
            //PSOsettings.Add("phi1", hpo_x[6]);                //x[6] ∈ [0,5], attraction own best 
            //PSOsettings.Add("phi2", hpo_x[7]);                //x[7] ∈ [0,5], attraction global best 
            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                PSO[i] = new MetaheuristicsLibrary.SolversSO.PSO(lb, ub, xint, _maxfuncs, testfunc, i, PSOsettings);
                PSO[i].solve();
                mins[i] = PSO[i].get_fxoptimum();
            }

            return mins.Average();
        }
        private double meanPSO(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> PSOsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] PSO = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int fipsopop = Convert.ToInt16(hpo_x[0]);
            PSOsettings.Add("popsize", fipsopop);             //x[0] ∈ {4,..., 100}
            PSOsettings.Add("chi", hpo_x[1]);                 //x[1] ∈ [0.001, 1], constriction coefficient
            //PSOsettings.Add("phi", hpo_x[2]);                 //x[2] ∈ [0, 50], attraction to best particle
            PSOsettings.Add("v0max", hpo_x[2]);               //x[3] ∈ [0, 10], initial velocity
            PSOsettings.Add("pxupdatemode", Convert.ToInt16(hpo_x[3])); //x[4] 0 = update after population. 1 = update after each evaluation 
            PSOsettings.Add("psomode", Convert.ToInt16(hpo_x[4])); //x[5] 0 = fipso, 1 = inertia, 2 = constriction 
            PSOsettings.Add("phi1", hpo_x[5]);                //x[6] ∈ [0,5], attraction own best 
            PSOsettings.Add("phi2", hpo_x[6]);                //x[7] ∈ [0,5], attraction global best 
            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                PSO[i] = new MetaheuristicsLibrary.SolversSO.PSO(lb, ub, xint, _maxfuncs, testfunc, i, PSOsettings);
                PSO[i].solve();
                mins[i] = PSO[i].get_fxoptimum();
            }

            return mins.Average();
        }

        private double switchMean(double[] x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            switch (solver)
            {
                case "ES":
                    return meanES(x, testfunc, _maxfuncs, lb, ub);
                case "SGA":
                    return meanSGA(x, testfunc, _maxfuncs, lb, ub);
                case "FIPSO":
                    return meanFIPSO(x, testfunc, _maxfuncs, lb, ub);
                case "PSO":
                    return meanPSO(x, testfunc, _maxfuncs, lb, ub);
                default:
                    return 0.0;
            }
        }


        //ES has 8 parameters
        private double []distES(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> settingsES = new Dictionary<string, object>();
            int simpleESpop = Convert.ToInt16(hpo_x[0]);
            int simpleESLambda = Convert.ToInt16(hpo_x[1]);
            int simpleESroh = Convert.ToInt16(hpo_x[2] * simpleESLambda) + 1; //from percentage to integer
            settingsES.Add("popsize", simpleESpop);          // ∈ {2,...,200}
            settingsES.Add("lambda", simpleESLambda);            // ∈ {1,...,200}
            settingsES.Add("roh", simpleESroh);               // ∈ {1,...,popsize}  . in hyperoptimization, express as percentage of lambda
            settingsES.Add("x0sampling", Convert.ToInt16(hpo_x[3]));        // ∈ {0,1}  0=uniform, 1=gaussian
            settingsES.Add("stepsize0", hpo_x[4]);       // ∈ [0.01, 10]
            settingsES.Add("stepsize", hpo_x[5]);        // ∈ [0.01, 10]
            settingsES.Add("tauc", hpo_x[6]);              // ∈ [0.01, 10]
            settingsES.Add("selmode", hpo_x[7]);        // 0 = random, 1=roulette wheel
            MetaheuristicsLibrary.SolversSO.SO_Solver[] simpleES = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];

            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                simpleES[i] = new MetaheuristicsLibrary.SolversSO.SimpleES(lb, ub, xint, _maxfuncs, testfunc, i, settingsES);
                simpleES[i].solve();
                mins[i] = simpleES[i].get_fxoptimum();
            }
            return mins;
        }

        //SGA has 7 parameters
        private double []distSGA(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> simpleGAsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] simpleGAs = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int simplegapop = Convert.ToInt32(hpo_x[0]);
            simplegapop += simplegapop % 2;
            simpleGAsettings.Add("popsize", simplegapop);            //x[0]  50
            simpleGAsettings.Add("maxgen", Convert.ToInt32(Math.Floor(_maxfuncs / Convert.ToDouble(simplegapop))));            //no x. its a function of popsize and max evals.  50
            simpleGAsettings.Add("k", hpo_x[1]);                //x[1]  6
            simpleGAsettings.Add("pcross", hpo_x[2]);           //x[2]  0.7
            simpleGAsettings.Add("pmut", hpo_x[3]);             //x[3]  0.3
            simpleGAsettings.Add("d", hpo_x[4]);                //x[4]  0.1
            simpleGAsettings.Add("r", hpo_x[5]);                //x[5]  0.1
            int SGAelite = Convert.ToInt16(hpo_x[6] * (int)Math.Round((double)simplegapop / 2, 0)); //from percentage to integer
            simpleGAsettings.Add("elite", SGAelite);                //x[6]  0 - (popsize/2). 

            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                simpleGAs[i] = new MetaheuristicsLibrary.SolversSO.SimpleGA(lb, ub, xint, _maxfuncs, testfunc, i, simpleGAsettings);
                simpleGAs[i].solve();
                mins[i] = simpleGAs[i].get_fxoptimum();
            }

            return mins;
        }

        private double []distFIPSO(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> PSOsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] PSO = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int fipsopop = Convert.ToInt16(hpo_x[0]);
            PSOsettings.Add("popsize", fipsopop);             //x[0] ∈ {4,..., 100}
            PSOsettings.Add("chi", hpo_x[1]);                 //x[1] ∈ [0.001, 1], constriction coefficient
            PSOsettings.Add("phi", hpo_x[2]);                 //x[2] ∈ [0, 50], attraction to best particle
            PSOsettings.Add("v0max", hpo_x[3]);               //x[3] ∈ [0, 10], initial velocity
            PSOsettings.Add("pxupdatemode", Convert.ToInt16(hpo_x[4])); //x[4] 0 = update after population. 1 = update after each evaluation 
            PSOsettings.Add("psomode", 0);
            //PSOsettings.Add("psomode", Convert.ToInt16(hpo_x[5])); //x[5] 0 = fipso, 1 = inertia, 2 = constriction 
            //PSOsettings.Add("phi1", hpo_x[6]);                //x[6] ∈ [0,5], attraction own best 
            //PSOsettings.Add("phi2", hpo_x[7]);                //x[7] ∈ [0,5], attraction global best 
            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                PSO[i] = new MetaheuristicsLibrary.SolversSO.PSO(lb, ub, xint, _maxfuncs, testfunc, i, PSOsettings);
                PSO[i].solve();
                mins[i] = PSO[i].get_fxoptimum();
            }

            return mins;
        }
        private double []distPSO(double[] hpo_x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            int dvar = testfuncDim;
            int runs = runsperTestfunc;

            bool[] xint = new bool[dvar];
            Dictionary<string, object> PSOsettings = new Dictionary<string, object>();
            MetaheuristicsLibrary.SolversSO.SO_Solver[] PSO = new MetaheuristicsLibrary.SolversSO.SO_Solver[runs];
            int fipsopop = Convert.ToInt16(hpo_x[0]);
            PSOsettings.Add("popsize", fipsopop);             //x[0] ∈ {4,..., 100}
            PSOsettings.Add("chi", hpo_x[1]);                 //x[1] ∈ [0.001, 1], constriction coefficient
            //PSOsettings.Add("phi", hpo_x[2]);                 //x[2] ∈ [0, 50], attraction to best particle
            PSOsettings.Add("v0max", hpo_x[2]);               //x[3] ∈ [0, 10], initial velocity
            PSOsettings.Add("pxupdatemode", Convert.ToInt16(hpo_x[3])); //x[4] 0 = update after population. 1 = update after each evaluation 
            PSOsettings.Add("psomode", Convert.ToInt16(hpo_x[4])); //x[5] 0 = fipso, 1 = inertia, 2 = constriction 
            PSOsettings.Add("phi1", hpo_x[5]);                //x[6] ∈ [0,5], attraction own best 
            PSOsettings.Add("phi2", hpo_x[6]);                //x[7] ∈ [0,5], attraction global best 
            double[] mins = new double[runs];
            for (int i = 0; i < runs; i++)
            {
                PSO[i] = new MetaheuristicsLibrary.SolversSO.PSO(lb, ub, xint, _maxfuncs, testfunc, i, PSOsettings);
                PSO[i].solve();
                mins[i] = PSO[i].get_fxoptimum();
            }

            return mins;
        }

        private double [] switchDist(double[] x, Func<double[], double> testfunc, int _maxfuncs, double[] lb, double[] ub)
        {
            switch (solver)
            {
                case "ES":
                    return distES(x, testfunc, _maxfuncs, lb, ub);
                case "SGA":
                    return distSGA(x, testfunc, _maxfuncs, lb, ub);
                case "FIPSO":
                    return distFIPSO(x, testfunc, _maxfuncs, lb, ub);
                case "PSO":
                    return distPSO(x, testfunc, _maxfuncs, lb, ub);
                default:
                    return new double [lb.Length];
            }
        }




        



        internal double HyperFunc_L_Rastrigin(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Rastrigin;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.12;
                ub[i] = 5.12;
            }
            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Levy(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Levy;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10;
                ub[i] = 10;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Griewank(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Griewank;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -600;
                ub[i] = 600;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Ackley(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Ackley;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -32.768;
                ub[i] = 32.768;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Schwefel(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Schwefel;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -500.0;
                ub[i] = 500.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_P_Zhakarov(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.P_Zakharov;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.0;
                ub[i] = 10.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_V_Rosenbrock(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_Rosenbrock;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.0;
                ub[i] = 10.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_V_DixonPrice(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_DixonPrice;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10.0;
                ub[i] = 10.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_Sphere(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Sphere;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.12;
                ub[i] = 5.12;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_SumSquares(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_SumSquares;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10.0;
                ub[i] = 10.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_Trid(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Trid;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = Math.Pow(dvar, 2) * -1;
                ub[i] = Math.Pow(dvar, 2);
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_RotHypEll(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_RotHypEll;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -65.536;
                ub[i] = 65.536;
            }
            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_Perm0db(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Perm0db;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -dvar;
                ub[i] = dvar;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_O_PermDB(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_PermDB;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -dvar;
                ub[i] = dvar;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_O_StyblinskiTang(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_StyblinskiTang;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5;
                ub[i] = 5;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }








        internal double HyperFunc_L_Rastrigin_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Rastrigin;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 5.12 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Levy_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Levy;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
            //double m = switchMean(x, testfunc, _maxfuncs, lb, ub);
            //Console.WriteLine(m);
            //logs.Add(Convert.ToString(m));
            //return m;
        }

        internal double HyperFunc_L_Griewank_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Griewank;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 600 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Ackley_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Ackley;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 32.768 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_L_Schwefel_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Schwefel;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 500.0 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_P_Zhakarov_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.P_Zakharov;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 15.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_V_Rosenbrock_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_Rosenbrock;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 15.0;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_V_DixonPrice_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_DixonPrice;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10.0 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_Sphere_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Sphere;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 5.12 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_SumSquares_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_SumSquares;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10.0 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_B_RotHypEll_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_RotHypEll;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 65.536 * 2;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double HyperFunc_O_StyblinskiTang_Edge(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_StyblinskiTang;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -3;
                ub[i] = 8;
            }

            return switchMean(x, testfunc, _maxfuncs, lb, ub);
        }













        internal double [] HyperFunc_L_Rastrigin_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Rastrigin;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.12;
                ub[i] = 5.12;
            }
            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_L_Levy_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Levy;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10;
                ub[i] = 10;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_L_Griewank_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Griewank;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -600;
                ub[i] = 600;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_L_Ackley_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Ackley;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -32.768;
                ub[i] = 32.768;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_L_Schwefel_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Schwefel;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -500.0;
                ub[i] = 500.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_P_Zhakarov_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.P_Zakharov;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.0;
                ub[i] = 10.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_V_Rosenbrock_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_Rosenbrock;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.0;
                ub[i] = 10.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_V_DixonPrice_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_DixonPrice;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10.0;
                ub[i] = 10.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_B_Sphere_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Sphere;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5.12;
                ub[i] = 5.12;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double [] HyperFunc_B_SumSquares_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_SumSquares;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -10.0;
                ub[i] = 10.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_Trid_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Trid;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = Math.Pow(dvar, 2) * -1;
                ub[i] = Math.Pow(dvar, 2);
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_RotHypEll_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_RotHypEll;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -65.536;
                ub[i] = 65.536;
            }
            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_Perm0db_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Perm0db;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -dvar;
                ub[i] = dvar;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double []HyperFunc_O_PermDB_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_PermDB;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -dvar;
                ub[i] = dvar;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double []HyperFunc_O_StyblinskiTang_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_StyblinskiTang;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -5;
                ub[i] = 5;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }








        internal double []HyperFunc_L_Rastrigin_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Rastrigin;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 5.12 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double []HyperFunc_L_Levy_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Levy;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
            //double m = switchMean(x, testfunc, _maxfuncs, lb, ub);
            //Console.WriteLine(m);
            //logs.Add(Convert.ToString(m));
            //return m;
        }

        internal double[] HyperFunc_L_Griewank_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Griewank;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 600 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_L_Ackley_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Ackley;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 32.768 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_L_Schwefel_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.L_Schwefel;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 500.0 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_P_Zhakarov_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.P_Zakharov;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 15.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_V_Rosenbrock_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_Rosenbrock;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 15.0;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_V_DixonPrice_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.V_DixonPrice;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10.0 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_Sphere_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_Sphere;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 5.12 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_SumSquares_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_SumSquares;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 10.0 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_B_RotHypEll_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.B_RotHypEll;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = 0;
                ub[i] = 65.536 * 2;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }

        internal double[] HyperFunc_O_StyblinskiTang_Edge_dist(double[] x)
        {
            int dvar = testfuncDim;

            Func<double[], double> testfunc = SO.O_StyblinskiTang;
            int _maxfuncs = (dvar + 1) * evalbudget;   //max func evals
            double[] lb = new double[dvar];
            double[] ub = new double[dvar];
            for (int i = 0; i < dvar; i++)
            {
                lb[i] = -3;
                ub[i] = 8;
            }

            return switchDist(x, testfunc, _maxfuncs, lb, ub);
        }
    }
}
