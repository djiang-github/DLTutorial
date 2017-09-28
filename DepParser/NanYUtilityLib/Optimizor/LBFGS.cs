using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib.Optimizor
{
    public enum LineSearchScheme
    {
        Morethuente,
        Backtracking_Armijo,
        Backtracking_Wolfe,
        Backtracking_StrongWolfe
    }

    public class LBFGSParam
    {
        public LBFGSParam()
        {
            m = 6;
            epsilon = 1e-5f;
            past = 0;
            delta = 0;
            max_iterations = 0;
            linesearch = LineSearchScheme.Morethuente;
            max_linesearch = 20;
            min_step = 1e-20f;
            max_step = 1e+20f;
            ftol = 1e-4f;
            wolfe = 0.9f;
            gtol = 0.9f;
            xtol = 1e-30f;
            orthantwise_c = 0;
            orthantwise_start = 0;
            orthantwise_end = -1;
        }

        /**
         * The number of corrections to approximate the inverse hessian matrix.
         *  The L-BFGS routine stores the computation results of previous \ref m
         *  iterations to approximate the inverse hessian matrix of the current
         *  iteration. This parameter controls the size of the limited memories
         *  (corrections). The default value is \c 6. Values less than \c 3 are
         *  not recommended. Large values will result in excessive computing time.
        */
        public int m;

        /**
         * Epsilon for convergence test.
         *  This parameter determines the accuracy with which the solution is to
         *  be found. A minimization terminates when
         *      ||g|| < \ref epsilon * max(1, ||x||),
         *  where ||.|| denotes the Euclidean (L2) norm. The default value is
         *  \c 1e-5.
         */
        public float epsilon;

        /**
         * Distance for delta-based convergence test.
         *  This parameter determines the distance, in iterations, to compute
         *  the rate of decrease of the objective function. If the value of this
         *  parameter is zero, the library does not perform the delta-based
         *  convergence test. The default value is \c 0.
         */
        public int past;

        /**
         * Delta for convergence test.
         *  This parameter determines the minimum rate of decrease of the
         *  objective function. The library stops iterations when the
         *  following condition is met:
         *      (f' - f) / f < \ref delta,
         *  where f' is the objective value of \ref past iterations ago, and f is
         *  the objective value of the current iteration.
         *  The default value is \c 0.
         */
        public float delta;

        /**
         * The maximum number of iterations.
         *  The lbfgs() function terminates an optimization process with
         *  ::LBFGSERR_MAXIMUMITERATION status code when the iteration count
         *  exceedes this parameter. Setting this parameter to zero continues an
         *  optimization process until a convergence or error. The default value
         *  is \c 0.
         */
        public int max_iterations;

        /**
         * The line search algorithm.
         *  This parameter specifies a line search algorithm to be used by the
         *  L-BFGS routine.
         */
        public LineSearchScheme linesearch;

        /**
         * The maximum number of trials for the line search.
         *  This parameter controls the number of function and gradients evaluations
         *  per iteration for the line search routine. The default value is \c 20.
         */
        public int max_linesearch;

        /**
         * The minimum step of the line search routine.
         *  The default value is \c 1e-20. This value need not be modified unless
         *  the exponents are too large for the machine being used, or unless the
         *  problem is extremely badly scaled (in which case the exponents should
         *  be increased).
         */
        public float min_step;

        /**
         * The maximum step of the line search.
         *  The default value is \c 1e+20. This value need not be modified unless
         *  the exponents are too large for the machine being used, or unless the
         *  problem is extremely badly scaled (in which case the exponents should
         *  be increased).
         */
        public float max_step;

        /**
         * A parameter to control the accuracy of the line search routine.
         *  The default value is \c 1e-4. This parameter should be greater
         *  than zero and smaller than \c 0.5.
         */
        public float ftol;

        /**
         * A coefficient for the Wolfe condition.
         *  This parameter is valid only when the backtracking line-search
         *  algorithm is used with the Wolfe condition,
         *  ::LBFGS_LINESEARCH_BACKTRACKING_STRONG_WOLFE or
         *  ::LBFGS_LINESEARCH_BACKTRACKING_WOLFE .
         *  The default value is \c 0.9. This parameter should be greater
         *  the \ref ftol parameter and smaller than \c 1.0.
         */
        public float wolfe;

        /**
         * A parameter to control the accuracy of the line search routine.
         *  The default value is \c 0.9. If the function and gradient
         *  evaluations are inexpensive with respect to the cost of the
         *  iteration (which is sometimes the case when solving very large
         *  problems) it may be advantageous to set this parameter to a small
         *  value. A typical small value is \c 0.1. This parameter shuold be
         *  greater than the \ref ftol parameter (\c 1e-4) and smaller than
         *  \c 1.0.
         */
        public float gtol;

        /**
         * The machine precision for floating-point values.
         *  This parameter must be a positive value set by a client program to
         *  estimate the machine precision. The line search routine will terminate
         *  with the status code (::LBFGSERR_ROUNDING_ERROR) if the relative width
         *  of the interval of uncertainty is less than this parameter.
         */
        public float xtol;

        /**
         * Coeefficient for the L1 norm of variables.
         *  This parameter should be set to zero for standard minimization
         *  problems. Setting this parameter to a positive value activates
         *  Orthant-Wise Limited-memory Quasi-Newton (OWL-QN) method, which
         *  minimizes the objective function F(x) combined with the L1 norm |x|
         *  of the variables, {F(x) + C |x|}. This parameter is the coeefficient
         *  for the |x|, i.e., C. As the L1 norm |x| is not differentiable at
         *  zero, the library modifies function and gradient evaluations from
         *  a client program suitably; a client program thus have only to return
         *  the function value F(x) and gradients G(x) as usual. The default value
         *  is zero.
         */
        public float orthantwise_c;

        /**
         * Start index for computing L1 norm of the variables.
         *  This parameter is valid only for OWL-QN method
         *  (i.e., \ref orthantwise_c != 0). This parameter b (0 <= b < N)
         *  specifies the index number from which the library computes the
         *  L1 norm of the variables x,
         *      |x| := |x_{b}| + |x_{b+1}| + ... + |x_{N}| .
         *  In other words, variables x_1, ..., x_{b-1} are not used for
         *  computing the L1 norm. Setting b (0 < b < N), one can protect
         *  variables, x_1, ..., x_{b-1} (e.g., a bias term of logistic
         *  regression) from being regularized. The default value is zero.
         */
        public int orthantwise_start;

        /**
         * End index for computing L1 norm of the variables.
         *  This parameter is valid only for OWL-QN method
         *  (i.e., \ref orthantwise_c != 0). This parameter e (0 < e <= N)
         *  specifies the index number at which the library stops computing the
         *  L1 norm of the variables x,
         */
        public int orthantwise_end;

    }

    public class LBFGS
    {
        /// <summary>
        /// User defined function to compute gradient(x) and f(x)
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="grad">gradient to be computed</param>
        /// <returns>f(x)</returns>
        public delegate float compute_f_and_grad_delegate(float[] x, float[] grad);

        /// <summary>
        /// Report optimization progress. User can terminate optimization by return non-zero values.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="fx">f(x)</param>
        /// <param name="iter">current iteration number</param>
        /// <returns>Return non-zero value to stop optimization</returns>
        public delegate int progress_report_delegate(float[] x, float fx, int iter);

        /// <summary>
        /// Search suitable point along the given line.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <param name="s"></param>
        /// <param name="stp"></param>
        /// <param name="xp"></param>
        /// <param name="gp"></param>
        /// <param name="wa"></param>
        /// <param name="compute_f_and_grad"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private delegate int line_search_proc_delegate(
            float[] x,
            ref float f,
            float[] g,
            float[] s,
            ref float stp,
            float[] xp,
            float[] gp,
            float[] wa,
            compute_f_and_grad_delegate compute_f_and_grad,
            LBFGSParam param
            );
        /// <summary>
        /// Storing data from previous iteration.
        /// It is used to test convergence and to guess current inverse Hessian
        /// </summary>
        private class iteration_data
        {
            public float alpha;
            public float[] s;
            public float[] y;
            public float ys;
        }


        public LBFGS(LBFGSParam param,
            compute_f_and_grad_delegate compute_f_and_grad,
            progress_report_delegate progress_report)
        {
            this.param = param;
            this.compute_f_and_grad = compute_f_and_grad;
            this.progress_report = progress_report;
        }

        public void Run(float[] x, out float fx)
        {
            if (x == null || x.Length == 0)
            {
                throw new Exception("Invalid input vector x");
            }

            this.x = x;

            InitParam(param);

            AllocateBuffer();

            fx = compute_f_and_grad(x, gradient);
            float xnorm = 0;
            float gnorm = 0;

            if (IsOWLQN)
            {
                xnorm = owlqn_x1norm(this.x, param.orthantwise_start, param.orthantwise_end);

                fx += xnorm * param.orthantwise_c;

                owlqn_pseudo_gradient(
                    projected_gradient, x, gradient,
                    param.orthantwise_c, param.orthantwise_start, param.orthantwise_end
                    );
            }

            if (previous_f != null)
            {
                previous_f[0] = fx;
            }

            // compute direction
            // assume the intial hessian matrix is the identity matrix
            if (!IsOWLQN)
            {
                vectorop.vecncpy(search_direction, gradient);
            }
            else
            {
                vectorop.vecncpy(search_direction, projected_gradient);
            }

            // make sure the initial variables are not minimal
            xnorm = vectorop.vec2norm(x);

            gnorm = IsOWLQN ? vectorop.vec2norm(projected_gradient) : vectorop.vec2norm(gradient);

            if (xnorm < 1.0)
            {
                xnorm = 1.0f;
            }

            if (gnorm / xnorm <= param.epsilon)
            {
                return;
            }
            // compute intial step
            // step = 1.0 / ||d||
            float step = vectorop.vec2norminv(search_direction);

            int k = 1;
            int end = 0;

            for (; ; )
            {
                // store current position and grad
                vectorop.veccpy(previous_x, x);
                vectorop.veccpy(previous_gradient, gradient);

                // search for optimal step
                int ls = 0;
                if (!IsOWLQN)
                {
                    ls = line_search(
                        x, ref fx, gradient, search_direction, ref step, previous_x, previous_gradient, w, compute_f_and_grad, param);
                }
                else
                {
                    ls = line_search(
                        x, ref fx, gradient, search_direction, ref step, previous_x, projected_gradient, w, compute_f_and_grad, param);
                    owlqn_pseudo_gradient(
                        projected_gradient, x, gradient,
                        param.orthantwise_c, param.orthantwise_start, param.orthantwise_end);
                }
                if (ls < 0)
                {
                    // search fail to find better point
                    // reverse to previous point
                    vectorop.veccpy(x, previous_x);
                    vectorop.veccpy(gradient, previous_gradient);
                    Console.Error.WriteLine("line search fail to reach a better point.");
                    return;
                }

                xnorm = vectorop.vec2norm(x);

                gnorm = IsOWLQN ? vectorop.vec2norm(projected_gradient) : vectorop.vec2norm(gradient);

                if (progress_report != null)
                {
                    if (progress_report(x, fx, k) != 0)
                    {
                        return;
                    }
                }

                // test convergence

                if (xnorm < 1.0)
                {
                    xnorm = 1.0f;
                }

                if (gnorm / xnorm <= param.epsilon)
                {
                    return;
                }

                // test stopping criterion

                if (previous_f != null)
                {
                    // don't test first several iterations
                    if (param.past <= k)
                    {
                        float rate = (previous_f[k % param.past] - fx) / fx;

                        if (rate < param.delta)
                        {
                            break;
                        }
                    }

                    previous_f[k % param.past] = fx;
                }

                if (param.max_iterations != 0 && param.max_iterations < k + 1)
                {
                    break;
                }

                // update cache;
                var it = lm[end];
                vectorop.vecdiff(it.s, x, previous_x);
                vectorop.vecdiff(it.y, gradient, previous_gradient);

                float ys = vectorop.vecdot(it.y, it.s);
                float yy = vectorop.vecdot(it.y, it.y);
                it.ys = ys;

                // compute dir = - H * g

                int bound = (m <= k) ? m : k;
                ++k;
                end = (end + 1) % m;

                if (!IsOWLQN)
                {
                    vectorop.vecncpy(search_direction, gradient);
                }
                else
                {
                    vectorop.vecncpy(search_direction, projected_gradient);
                }

                int j = end;
                for (int i = 0; i < bound; ++i)
                {
                    j = (j + m - 1) % m;
                    it = lm[j];

                    it.alpha = vectorop.vecdot(it.s, search_direction);
                    it.alpha /= it.ys;

                    vectorop.vecadd(search_direction, it.y, -it.alpha);
                }

                vectorop.vecscale(search_direction, ys / yy);

                for (int i = 0; i < bound; ++i)
                {
                    it = lm[j];

                    float beta = vectorop.vecdot(it.y, search_direction);
                    beta /= it.ys;

                    vectorop.vecadd(search_direction, it.s, it.alpha - beta);
                    j = (j + 1) % m;
                }

                // constrain search direction for OW update
                if (IsOWLQN)
                {
                    for (int i = param.orthantwise_start; i < param.orthantwise_end; ++i)
                    {
                        if (search_direction[i] * projected_gradient[i] >= 0)
                        {
                            search_direction[i] = 0;
                        }
                    }
                }

                step = 1.0f;
            }

        }

        /// <summary>
        /// Allocating memory for LBFGS optimizor.
        /// </summary>
        private void AllocateBuffer()
        {
            previous_x = new float[n];
            gradient = new float[n];
            previous_gradient = new float[n];
            search_direction = new float[n];
            w = new float[n];

            if (param.orthantwise_c != 0)
            {
                projected_gradient = new float[n];
            }

            lm = new iteration_data[m];

            for (int i = 0; i < lm.Length; ++i)
            {
                lm[i] = new iteration_data
                {
                    alpha = 0,
                    ys = 0,
                    s = new float[n],
                    y = new float[n]
                };
            }

            if (0 < param.past)
            {
                previous_f = new float[param.past];
            }
        }

        /// <summary>
        /// Compute the L1-norm for vector x from start to end.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        static private float owlqn_x1norm(float[] x, int start, int end)
        {
            float norm = 0.0f;

            for (int i = start; i < end; ++i)
            {
                norm += Math.Abs(x[i]);
            }

            return norm;
        }

        /// <summary>
        /// Project the input vector to the same orthant to sign
        /// </summary>
        /// <param name="x"></param>
        /// <param name="sign"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        static private void owlqn_project(float[] x, float[] sign, int start, int end)
        {
            for (int i = start; i < end; ++i)
            {
                if (x[i] * sign[i] <= 0)
                {
                    x[i] = 0;
                }
            }
        }


        /// <summary>
        /// Compute the pseudo gradient for f(x) + c|x|
        /// </summary>
        /// <param name="pg">pseudo gradient to be computed</param>
        /// <param name="x">input point x</param>
        /// <param name="g">gradient for f(x)</param>
        /// <param name="c">L1-regularization C</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        static private void owlqn_pseudo_gradient(
            float[] pg,
            float[] x,
            float[] g,
            float c,
            int start,
            int end
            )
        {
            int i;

            for (i = 0; i < start; ++i)
            {
                pg[i] = g[i];
            }

            /* Compute the psuedo-gradients. */
            for (i = start; i < end; ++i)
            {
                if (x[i] < 0.0)
                {
                    /* Differentiable. */
                    pg[i] = g[i] - c;
                }
                else if (0.0 < x[i])
                {
                    /* Differentiable. */
                    pg[i] = g[i] + c;
                }
                else
                {
                    if (g[i] < -c)
                    {
                        /* Take the right partial derivative. */
                        pg[i] = g[i] + c;
                    }
                    else if (c < g[i])
                    {
                        /* Take the left partial derivative. */
                        pg[i] = g[i] - c;
                    }
                    else
                    {
                        pg[i] = 0.0f;
                    }
                }
            }

            for (i = end; i < pg.Length; ++i)
            {
                pg[i] = g[i];
            }
        }

        private void InitParam(LBFGSParam param)
        {
            if (param.epsilon < 0 ||
                param.past < 0 ||
                param.delta < 0 ||
                param.min_step < 0 ||
                param.max_step < param.min_step ||
                param.ftol < 0)
            {
                throw new Exception("Invalid param");
            }

            if (param.linesearch == LineSearchScheme.Backtracking_Wolfe
                || param.linesearch == LineSearchScheme.Backtracking_StrongWolfe)
            {
                if (param.wolfe <= param.ftol || 1.0 <= param.wolfe)
                {
                    throw new Exception("Invalid Wolfe parameter!");
                }
            }

            if (param.gtol < 0 || param.xtol < 0
                || param.max_linesearch <= 0
                || param.orthantwise_c < 0
                || param.orthantwise_start < 0
                || param.orthantwise_start > n
                || n < param.orthantwise_end)
            {
                throw new Exception("Invalid param");
            }

            if (param.orthantwise_end < 0)
            {
                param.orthantwise_end = n;
            }

            if (param.orthantwise_c != 0)
            {
                if (param.linesearch == LineSearchScheme.Backtracking_Wolfe)
                {
                    line_search = line_search_backtracking_owlqn;
                }
                else
                {
                    throw new Exception("Only wolfe linesearch support OWLQN");
                }
            }
            else
            {
                if (param.linesearch == LineSearchScheme.Morethuente)
                {
                    line_search = line_search_morethuente;
                }
                else
                {
                    line_search = line_search_backtracking;
                }
            }
        }

        LBFGSParam param;
        compute_f_and_grad_delegate compute_f_and_grad;
        progress_report_delegate progress_report;
        line_search_proc_delegate line_search;

        /// <summary>
        /// line search for OWLQN using back-tracking.
        /// </summary>
        /// <param name="x">x to be computed</param>
        /// <param name="f">f(x)</param>
        /// <param name="grad">gradient at x</param>
        /// <param name="search_dir">line search direction</param>
        /// <param name="stepsize">step size</param>
        /// <param name="start_x">start point</param>
        /// <param name="start_grad">pseudo-gradient of start point</param>
        /// <param name="orthant">orthant indicator</param>
        /// <param name="compute_f_and_grad">callback defined by user</param>
        /// <param name="param"></param>
        /// <returns>negative value indicates failure</returns>
        static private int line_search_backtracking_owlqn(
            float[] x,
            ref float f,
            float[] grad,
            float[] search_dir,
            ref float stepsize,
            float[] start_x,
            float[] start_grad,
            float[] orthant,
            compute_f_and_grad_delegate compute_f_and_grad,
            LBFGSParam param
            )
        {
            //int i, count = 0;
            float width = (float)0.5;// norm = 0.0;
            float finit = f;//, dgtest;
            int dim = x.Length;
            /* Check the input parameters for errors. */
            if (stepsize <= 0.0)
            {
                throw new Exception("invalid parameter");
            }

            /* Choose the orthant for the new point. */
            for (int i = 0; i < dim; ++i)
            {
                orthant[i] = (start_x[i] == 0.0) ? -start_grad[i] : start_x[i];
            }

            int count = 0;
            for (; ; )
            {
                /* Update the current point. */
                vectorop.veccpy(x, start_x);
                vectorop.vecadd(x, search_dir, stepsize);

                /* The current point is projected onto the orthant. */
                owlqn_project(x, orthant, param.orthantwise_start, param.orthantwise_end);

                /* Evaluate the function and gradient values. */
                f = compute_f_and_grad(x, grad);

                /* Compute the L1 norm of the variables and add it to the object value. */
                float norm = owlqn_x1norm(x, param.orthantwise_start, param.orthantwise_end);
                f += norm * param.orthantwise_c;

                ++count;

                // inner product of delta x and gradient.
                // dgtest = grad(start_x) dot (x - start_x)
                float dgtest = (float)0.0;
                for (int i = 0; i < dim; ++i)
                {
                    dgtest += (x[i] - start_x[i]) * start_grad[i];
                }

                if (f <= finit + param.ftol * dgtest)
                {
                    /* The sufficient decrease condition. */
                    return count;
                }

                if (stepsize < param.min_step)
                {
                    /* The step is the minimum value. */
                    return LBFGSERR_MINIMUMSTEP;
                }
                if (stepsize > param.max_step)
                {
                    /* The step is the maximum value. */
                    return LBFGSERR_MAXIMUMSTEP;
                }
                if (param.max_linesearch <= count)
                {
                    /* Maximum number of iteration. */
                    return LBFGSERR_MAXIMUMLINESEARCH;
                }

                stepsize *= width;
            }
        }

        static private int line_search_morethuente(
            float[] x,
            ref float f,
            float[] g,
            float[] s,
            ref float stp,
            float[] xp,
            float[] gp,
            float[] wa,
            compute_f_and_grad_delegate compute_f_and_grad,
            LBFGSParam param
            )
        {
            int count = 0;
            int brackt, stage1, uinfo = 0;
            float dg;
            float stx, fx, dgx;
            float sty, fy, dgy;
            float fxm, dgxm, fym, dgym, fm, dgm;
            float finit, ftest1, dginit, dgtest;
            float width, prev_width;
            float stmin, stmax;

            /* Check the input parameters for errors. */
            if (stp <= 0.0)
            {
                throw new Exception("Invalid Param");
            }

            /* Compute the initial gradient in the search direction. */
            dginit = vectorop.vecdot(g, s);

            /* Make sure that s points to a descent direction. */
            if (0 < dginit)
            {
                throw new Exception("ascending grad");
            }

            /* Initialize local variables. */
            brackt = 0;
            stage1 = 1;
            finit = f;
            dgtest = param.ftol * dginit;
            width = param.max_step - param.min_step;
            prev_width = 2.0f * width;

            /*
                The variables stx, fx, dgx contain the values of the step,
                function, and directional derivative at the best step.
                The variables sty, fy, dgy contain the value of the step,
                function, and derivative at the other endpoint of
                the interval of uncertainty.
                The variables stp, f, dg contain the values of the step,
                function, and derivative at the current step.
            */
            stx = sty = 0.0f;
            fx = fy = finit;
            dgx = dgy = dginit;

            for (; ; )
            {
                /*
                    Set the minimum and maximum steps to correspond to the
                    present interval of uncertainty.
                 */
                if (brackt != 0)
                {
                    stmin = Math.Min(stx, sty);
                    stmax = Math.Max(stx, sty);
                }
                else
                {
                    stmin = stx;
                    stmax = stp + 4.0f * (stp - stx);
                }

                /* Clip the step in the range of [stpmin, stpmax]. */
                if (stp < param.min_step)
                {
                    stp = param.min_step;
                }
                if (param.max_step < stp)
                {
                    stp = param.max_step;
                }

                /*
                    If an unusual termination is to occur then let
                    stp be the lowest point obtained so far.
                 */
                if ((brackt != 0 && ((stp <= stmin || stmax <= stp) || param.max_linesearch <= count + 1 || uinfo != 0)) || (brackt != 0 && (stmax - stmin <= param.xtol * stmax)))
                {
                    stp = stx;
                }

                /*
                    Compute the current value of x:
                        x <- x + (*stp) * s.
                 */
                vectorop.veccpy(x, xp);
                vectorop.vecadd(x, s, stp);

                /* Evaluate the function and gradient values. */
                f = compute_f_and_grad(x, g);
                dg = vectorop.vecdot(g, s);

                ftest1 = finit + stp * dgtest;
                ++count;

                /* Test for errors and convergence. */
                if (brackt != 0 && ((stp <= stmin || stmax <= stp) || uinfo != 0))
                {
                    /* Rounding errors prevent further progress. */
                    return LBFGSERR_ROUNDING_ERROR;
                }
                if (stp == param.max_step && f <= ftest1 && dg <= dgtest)
                {
                    /* The step is the maximum value. */
                    return LBFGSERR_MAXIMUMSTEP;
                }
                if (stp == param.min_step && (ftest1 < f || dgtest <= dg))
                {
                    /* The step is the minimum value. */
                    return LBFGSERR_MINIMUMSTEP;
                }
                if (brackt != 0 && (stmax - stmin) <= param.xtol * stmax)
                {
                    /* Relative width of the interval of uncertainty is at most xtol. */
                    return LBFGSERR_WIDTHTOOSMALL;
                }
                if (param.max_linesearch <= count)
                {
                    /* Maximum number of iteration. */
                    return LBFGSERR_MAXIMUMLINESEARCH;
                }
                if (f <= ftest1 && Math.Abs(dg) <= param.gtol * (-dginit))
                {
                    /* The sufficient decrease condition and the directional derivative condition hold. */
                    return count;
                }

                /*
                    In the first stage we seek a step for which the modified
                    function has a nonpositive value and nonnegative derivative.
                 */
                if (stage1 != 0 && f <= ftest1 && Math.Min(param.ftol, param.gtol) * dginit <= dg)
                {
                    stage1 = 0;
                }

                /*
                    A modified function is used to predict the step only if
                    we have not obtained a step for which the modified
                    function has a nonpositive function value and nonnegative
                    derivative, and if a lower function value has been
                    obtained but the decrease is not sufficient.
                 */
                if (stage1 != 0 && ftest1 < f && f <= fx)
                {
                    /* Define the modified function and derivative values. */
                    fm = f - stp * dgtest;
                    fxm = fx - stx * dgtest;
                    fym = fy - sty * dgtest;
                    dgm = dg - dgtest;
                    dgxm = dgx - dgtest;
                    dgym = dgy - dgtest;

                    /*
                        Call update_trial_interval() to update the interval of
                        uncertainty and to compute the new step.
                     */
                    uinfo = update_trial_interval(
                        ref stx, ref fxm, ref dgxm,
                        ref sty, ref fym, ref dgym,
                        ref stp, ref fm, ref dgm,
                        stmin, stmax, ref brackt
                        );

                    /* Reset the function and gradient values for f. */
                    fx = fxm + stx * dgtest;
                    fy = fym + sty * dgtest;
                    dgx = dgxm + dgtest;
                    dgy = dgym + dgtest;
                }
                else
                {
                    /*
                        Call update_trial_interval() to update the interval of
                        uncertainty and to compute the new step.
                     */
                    uinfo = update_trial_interval(
                        ref stx, ref fx, ref dgx,
                        ref sty, ref fy, ref dgy,
                        ref stp, ref f, ref dg,
                        stmin, stmax, ref brackt
                        );
                }

                /*
                    Force a sufficient decrease in the interval of uncertainty.
                 */
                if (brackt != 0)
                {
                    if (0.66 * prev_width <= Math.Abs(sty - stx))
                    {
                        stp = stx + 0.5f * (sty - stx);
                    }
                    prev_width = width;
                    width = Math.Abs(sty - stx);
                }
            }
        }

        static private int line_search_backtracking(
            float[] x,
            ref float f,
            float[] g,
            float[] s,
            ref float stp,
            float[] xp,
            float[] gp,
            float[] wa,
            compute_f_and_grad_delegate compute_f_and_grad,
            LBFGSParam param
            )
        {
            int count = 0;
            float width, dg;
            float finit, dginit = 0.0f, dgtest;
            const float dec = 0.5f, inc = 2.1f;

            /* Check the input parameters for errors. */
            if (stp <= 0.0)
            {
                throw new Exception("Invalid parameter");
            }

            /* Compute the initial gradient in the search direction. */
            dginit = vectorop.vecdot(g, s);

            /* Make sure that s points to a descent direction. */
            if (0 < dginit)
            {
                throw new Exception("Grad is not descending!");
            }

            /* The initial value of the objective function. */
            finit = f;
            dgtest = param.ftol * dginit;

            for (; ; )
            {
                vectorop.veccpy(x, xp);
                vectorop.vecadd(x, s, stp);

                /* Evaluate the function and gradient values. */
                f = compute_f_and_grad(x, g);

                ++count;

                if (f > finit + stp * dgtest)
                {
                    width = dec;
                }
                else
                {
                    /* The sufficient decrease condition (Armijo condition). */
                    if (param.linesearch == LineSearchScheme.Backtracking_Armijo)
                    {
                        /* Exit with the Armijo condition. */
                        return count;
                    }

                    /* Check the Wolfe condition. */
                    dg = vectorop.vecdot(g, s);
                    if (dg < param.wolfe * dginit)
                    {
                        width = inc;
                    }
                    else
                    {
                        if (param.linesearch == LineSearchScheme.Backtracking_Wolfe)
                        {
                            /* Exit with the regular Wolfe condition. */
                            return count;
                        }

                        /* Check the strong Wolfe condition. */
                        if (dg > -param.wolfe * dginit)
                        {
                            width = dec;
                        }
                        else
                        {
                            /* Exit with the strong Wolfe condition. */
                            return count;
                        }
                    }
                }

                if (stp < param.min_step)
                {
                    /* The step is the minimum value. */
                    return 0;
                }
                if (stp > param.max_step)
                {
                    /* The step is the maximum value. */
                    return 0;
                }
                if (param.max_linesearch <= count)
                {
                    /* Maximum number of iteration. */
                    return 0;
                }

                stp *= width;

            }

        }

        static float CUBIC_MINIMIZER(//ref float cm,
            float u, float fu, float du,
            float v, float fv, float dv)
        {
            float a, d, gamma, theta, p, q, r, s;

            //float cm = 0;

            d = v - u;
            theta = (fu - fv) * 3 / d + du + dv;
            p = Math.Abs(theta);
            q = Math.Abs(du);
            r = Math.Abs(dv);
            s = Math.Max(p, Math.Max(q, r));
            a = theta / s;
            gamma = s * (float)Math.Sqrt(a * a - (du / s) * (dv / s));
            if (v < u)
            {
                gamma = -gamma;
            }

            p = gamma - (du) + theta;
            q = gamma - (du) + gamma + (dv);

            r = p / q;
            return u + r * d;
        }

        static float CUBIC_MINIMIZER2(//ref float cm,
            float u, float fu, float du,
            float v, float fv, float dv,
            float xmin, float xmax)
        {
            float a, d, gamma, theta, p, q, r, s;
            d = (v) - (u);
            theta = ((fu) - (fv)) * 3 / d + (du) + (dv);
            p = Math.Abs(theta);
            q = Math.Abs(du);
            r = Math.Abs(dv);
            s = Math.Max(p, Math.Max(q, r));
            /* gamma = s*sqrt((theta/s)**2 - (du/s) * (dv/s)) */
            a = theta / s;
            gamma = s * (float)Math.Sqrt(Math.Max(0, a * a - ((du) / s) * ((dv) / s)));
            if ((u) < (v)) gamma = -gamma;
            p = gamma - (dv) + theta;
            q = gamma - (dv) + gamma + (du);
            r = p / q;
            if (r < 0.0 && gamma != 0.0)
            {
                return (v) - r * d;
            }
            else if (a < 0)
            {
                return (xmax);
            }
            else
            {
                return (xmin);
            }
        }

        /**
 * Find a minimizer of an interpolated quadratic function.
 *  @param  qm      The minimizer of the interpolated quadratic.
 *  @param  u       The value of one point, u.
 *  @param  fu      The value of f(u).
 *  @param  du      The value of f'(u).
 *  @param  v       The value of another point, v.
 *  @param  fv      The value of f(v).
 */
        static float QUARD_MINIMIZER(//ref float qm,
            float u, float fu, float du,
            float v, float fv)
        {
            float a = (v) - (u);
            return (u) + (du) / (((fu) - (fv)) / a + (du)) / 2 * a;
        }

        /**
         * Find a minimizer of an interpolated quadratic function.
         *  @param  qm      The minimizer of the interpolated quadratic.
         *  @param  u       The value of one point, u.
         *  @param  du      The value of f'(u).
         *  @param  v       The value of another point, v.
         *  @param  dv      The value of f'(v).
         */
        static float QUARD_MINIMIZER2(//ref float qm,
            float u, float du,
            float v, float dv)
        {
            float a = (u) - (v);
            return (v) + (dv) / ((dv) - (du)) * a;
        }

        static int fsigndiff(float x, float y)
        {
            return x * (y / Math.Abs(y)) < 0.0 ? 1 : 0;
        }

        static int update_trial_interval(
            ref float x,
            ref float fx,
            ref float dx,
            ref float y,
            ref float fy,
            ref float dy,
            ref float t,
            ref float ft,
            ref float dt,
            float tmin,
            float tmax,
            ref int brackt
            )
        {
            int bound;
            int dsign = fsigndiff(dt, dx);
            float mc = 0; /* minimizer of an interpolated cubic. */
            float mq = 0; /* minimizer of an interpolated quadratic. */
            float newt = 0;   /* new trial value. */


            /* Check the input parameters for errors. */
            if (brackt != 0)
            {
                if (t <= Math.Min(x, y) || Math.Max(x, y) <= t)
                {
                    /* The trival value t is out of the interval. */
                    return LBFGSERR_OUTOFINTERVAL;
                }
                if (0.0 <= dx * (t - x))
                {
                    /* The function must decrease from x. */
                    return LBFGSERR_INCREASEGRADIENT;
                }
                if (tmax < tmin)
                {
                    /* Incorrect tmin and tmax specified. */
                    return LBFGSERR_INCORRECT_TMINMAX;
                }
            }

            /*
                Trial value selection.
             */
            if (fx < ft)
            {
                /*
                    Case 1: a higher function value.
                    The minimum is brackt. If the cubic minimizer is closer
                    to x than the quadratic one, the cubic one is taken, else
                    the average of the minimizers is taken.
                 */
                brackt = 1;
                bound = 1;
                mc = CUBIC_MINIMIZER(x, fx, dx, t, ft, dt);
                mq = QUARD_MINIMIZER(x, fx, dx, t, ft);
                if (Math.Abs(mc - x) < Math.Abs(mq - x))
                {
                    newt = mc;
                }
                else
                {
                    newt = mc + 0.5f * (mq - mc);
                }
            }
            else if (dsign != 0)
            {
                /*
                    Case 2: a lower function value and derivatives of
                    opposite sign. The minimum is brackt. If the cubic
                    minimizer is closer to x than the quadratic (secant) one,
                    the cubic one is taken, else the quadratic one is taken.
                 */
                brackt = 1;
                bound = 0;
                mc = CUBIC_MINIMIZER(x, fx, dx, t, ft, dt);
                mq = QUARD_MINIMIZER2(x, dx, t, dt);
                if (Math.Abs(mc - t) > Math.Abs(mq - t))
                {
                    newt = mc;
                }
                else
                {
                    newt = mq;
                }
            }
            else if (Math.Abs(dt) < Math.Abs(dx))
            {
                /*
                    Case 3: a lower function value, derivatives of the
                    same sign, and the magnitude of the derivative decreases.
                    The cubic minimizer is only used if the cubic tends to
                    infinity in the direction of the minimizer or if the minimum
                    of the cubic is beyond t. Otherwise the cubic minimizer is
                    defined to be either tmin or tmax. The quadratic (secant)
                    minimizer is also computed and if the minimum is brackt
                    then the the minimizer closest to x is taken, else the one
                    farthest away is taken.
                 */
                bound = 1;
                mc = CUBIC_MINIMIZER2(x, fx, dx, t, ft, dt, tmin, tmax);
                mq = QUARD_MINIMIZER2(x, dx, t, dt);
                if (brackt != 0)
                {
                    if (Math.Abs(t - mc) < Math.Abs(t - mq))
                    {
                        newt = mc;
                    }
                    else
                    {
                        newt = mq;
                    }
                }
                else
                {
                    if (Math.Abs(t - mc) > Math.Abs(t - mq))
                    {
                        newt = mc;
                    }
                    else
                    {
                        newt = mq;
                    }
                }
            }
            else
            {
                /*
                    Case 4: a lower function value, derivatives of the
                    same sign, and the magnitude of the derivative does
                    not decrease. If the minimum is not brackt, the step
                    is either tmin or tmax, else the cubic minimizer is taken.
                 */
                bound = 0;
                if (brackt != 0)
                {
                    newt = CUBIC_MINIMIZER(t, ft, dt, y, fy, dy);
                }
                else if (x < t)
                {
                    newt = tmax;
                }
                else
                {
                    newt = tmin;
                }
            }

            /*
                Update the interval of uncertainty. This update does not
                depend on the new step or the case analysis above.

                - Case a: if f(x) < f(t),
                    x <- x, y <- t.
                - Case b: if f(t) <= f(x) && f'(t)*f'(x) > 0,
                    x <- t, y <- y.
                - Case c: if f(t) <= f(x) && f'(t)*f'(x) < 0, 
                    x <- t, y <- x.
             */
            if (fx < ft)
            {
                /* Case a */
                y = t;
                fy = ft;
                dy = dt;
            }
            else
            {
                /* Case c */
                if (dsign != 0)
                {
                    y = x;
                    fy = fx;
                    dy = dx;
                }
                /* Cases b and c */
                x = t;
                fx = ft;
                dx = dt;
            }

            /* Clip the new trial value in [tmin, tmax]. */
            if (tmax < newt) newt = tmax;
            if (newt < tmin) newt = tmin;

            /*
                Redefine the new trial value if it is close to the upper bound
                of the interval.
             */
            if (brackt != 0 && bound != 0)
            {
                mq = x + 0.66f * (y - x);
                if (x < y)
                {
                    if (mq < newt) newt = mq;
                }
                else
                {
                    if (newt < mq) newt = mq;
                }
            }

            /* Return the new trial value. */
            t = newt;
            return 0;
        }



        float[] x;

        bool IsOWLQN { get { return 0 != param.orthantwise_c; } }
        int n { get { return x.Length; } }
        int m { get { return param.m; } }
        float[] previous_x;
        float[] gradient;
        float[] previous_gradient;
        float[] projected_gradient;
        float[] search_direction;
        float[] w;
        float[] previous_f;
        iteration_data[] lm;



        const int LBFGS_SUCCESS = 0;
        const int LBFGS_CONVERGENCE = 0;
        const int LBFGS_STOP = 1;
        /** The initial variables already minimize the objective function. */
        const int LBFGS_ALREADY_MINIMIZED = 2;

        /** Unknown error. */
        const int LBFGSERR_UNKNOWNERROR = -1024;
        /** Logic error. */
        const int LBFGSERR_LOGICERROR = -1023; // - 1023
        /** Insufficient memory. */
        const int LBFGSERR_OUTOFMEMORY = -1022; // -1022
        /** The minimization process has been canceled. */
        const int LBFGSERR_CANCELED = -1021; // -1021
        /** Invalid number of variables specified. */
        const int LBFGSERR_INVALID_N = -1020; // -1020
        /** Invalid number of variables (for SSE) specified. */
        const int LBFGSERR_INVALID_N_SSE = -1019; // -1019
        /** The array x must be aligned to 16 (for SSE). */
        const int LBFGSERR_INVALID_X_SSE = -1018; // -1018
        /** Invalid parameter lbfgs_parameter_t::epsilon specified. */
        const int LBFGSERR_INVALID_EPSILON = -1017; // -1017
        /** Invalid parameter lbfgs_parameter_t::past specified. */
        const int LBFGSERR_INVALID_TESTPERIOD = -1016; // -1016
        /** Invalid parameter lbfgs_parameter_t::delta specified. */
        const int LBFGSERR_INVALID_DELTA = -1015;
        /** Invalid parameter lbfgs_parameter_t::linesearch specified. */
        const int LBFGSERR_INVALID_LINESEARCH = -1014;
        /** Invalid parameter lbfgs_parameter_t::max_step specified. */
        const int LBFGSERR_INVALID_MINSTEP = -1013;
        /** Invalid parameter lbfgs_parameter_t::max_step specified. */
        const int LBFGSERR_INVALID_MAXSTEP = -1012;
        /** Invalid parameter lbfgs_parameter_t::ftol specified. */
        const int LBFGSERR_INVALID_FTOL = -1011;
        /** Invalid parameter lbfgs_parameter_t::wolfe specified. */
        const int LBFGSERR_INVALID_WOLFE = -1010;
        /** Invalid parameter lbfgs_parameter_t::gtol specified. */
        const int LBFGSERR_INVALID_GTOL = -1009;
        /** Invalid parameter lbfgs_parameter_t::xtol specified. */
        const int LBFGSERR_INVALID_XTOL = -1008;
        /** Invalid parameter lbfgs_parameter_t::max_linesearch specified. */
        const int LBFGSERR_INVALID_MAXLINESEARCH = -1007;
        /** Invalid parameter lbfgs_parameter_t::orthantwise_c specified. */
        const int LBFGSERR_INVALID_ORTHANTWISE = -1006;
        /** Invalid parameter lbfgs_parameter_t::orthantwise_start specified. */
        const int LBFGSERR_INVALID_ORTHANTWISE_START = -1005;
        /** Invalid parameter lbfgs_parameter_t::orthantwise_end specified. */
        const int LBFGSERR_INVALID_ORTHANTWISE_END = -1004;
        /** The line-search step went out of the interval of uncertainty. */
        const int LBFGSERR_OUTOFINTERVAL = -1003;
        /** A logic error occurred; alternatively, the interval of uncertainty
            became too small. */
        const int LBFGSERR_INCORRECT_TMINMAX = -1002;
        /** A rounding error occurred; alternatively, no line-search step
            satisfies the sufficient decrease and curvature conditions. */
        const int LBFGSERR_ROUNDING_ERROR = -1001;
        /** The line-search step became smaller than lbfgs_parameter_t::min_step. */
        const int LBFGSERR_MINIMUMSTEP = -1000;
        /** The line-search step became larger than lbfgs_parameter_t::max_step. */
        const int LBFGSERR_MAXIMUMSTEP = -999;
        /** The line-search routine reaches the maximum number of evaluations. */
        const int LBFGSERR_MAXIMUMLINESEARCH = -998;
        /** The algorithm routine reaches the maximum number of iterations. */
        const int LBFGSERR_MAXIMUMITERATION = -997;
        /** Relative width of the interval of uncertainty is at most
            lbfgs_parameter_t::xtol. */
        const int LBFGSERR_WIDTHTOOSMALL = -996;
        /** A logic error (negative line-search step) occurred. */
        const int LBFGSERR_INVALIDPARAMETERS = -995;
        /** The current search direction increases the objective function value. */
        const int LBFGSERR_INCREASEGRADIENT = -994;

    }

    static class vectorop
    {
        /// <summary>
        /// x[i] = c
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c"></param>
        public static void vecset(float[] x, float c)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < x.Length; ++i)
                {
                    x[i] = c;
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = x.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        x[i] = c;
                    }
                });
            }

        }

        /// <summary>
        /// dest = src
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void veccpy(float[] dest, float[] src)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    dest[i] = src[i];
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = dest.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        dest[i] = src[i];
                    }
                });
            }
            src.CopyTo(dest, 0);
        }

        /// <summary>
        /// dest = -src
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void vecncpy(float[] dest, float[] src)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    dest[i] = -src[i];
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = dest.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        dest[i] = -src[i];
                    }
                });
            }


        }

        /// <summary>
        /// dest += c * src
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="c"></param>
        public static void vecadd(float[] dest, float[] src, float c)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    dest[i] += c * src[i];
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = dest.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        dest[i] += c * src[i];
                    }
                });
            }


        }

        /// <summary>
        /// diff = x - y
        /// </summary>
        /// <param name="diff"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void vecdiff(float[] diff, float[] x, float[] y)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < diff.Length; ++i)
                {
                    diff[i] = x[i] - y[i];
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = diff.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        diff[i] = x[i] - y[i];
                    }
                });
            }


        }

        /// <summary>
        /// x *= c
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c"></param>
        public static void vecscale(float[] x, float c)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < x.Length; ++i)
                {
                    x[i] *= c;
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = x.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        x[i] *= c;
                    }
                });
            }
        }

        /// <summary>
        /// dest[i] *= src[i]
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void vecmul(float[] dest, float[] src)
        {
            if (nThread <= 1)
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    dest[i] *= src[i];
                }
            }
            else
            {
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = src.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        dest[i] *= src[i];
                    }
                });
            }

        }


        /// <summary>
        /// x dot y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float vecdot(float[] x, float[] y)
        {
            float sum = 0;
            if (nThread <= 1)
            {

                for (int i = 0; i < x.Length; ++i)
                {
                    sum += x[i] * y[i];
                }

                return sum;
            }
            else
            {
                float[] sums = new float[nThread];
                Parallel.For(0, nThread, (tid) =>
                {

                    int totalLoad = x.Length;
                    int averageLoad = totalLoad / nThread;

                    int start = averageLoad * tid;
                    int end = tid == nThread - 1 ? totalLoad : averageLoad * (tid + 1);
                    for (int i = start; i < end; ++i)
                    {
                        sums[tid] += x[i] * y[i];
                    }
                });
                foreach (float s in sums)
                {
                    sum += s;
                }
            }

            return sum;
        }

        /// <summary>
        /// ||x||
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float vec2norm(float[] x)
        {
            return (float)Math.Sqrt(vecdot(x, x));
        }

        /// <summary>
        /// 1 / ||x||
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float vec2norminv(float[] x)
        {
            return 1.0f / vec2norm(x);
        }

        public static int nThread = 1;
    }
}
