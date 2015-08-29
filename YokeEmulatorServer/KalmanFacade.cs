using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace YokeEmulatorServer
{
    public class KalmanFacade
    {
        #region inner class
        class KalmanFilter
        {
            int MP;                     /* number of measurement vector dimensions */
            int DP;                     /* number of state vector dimensions */
            int CP;                     /* number of control vector dimensions */

            public Matrix state_pre;           /* predicted state (x'(k)):
                                        x(k)=A*x(k-1)+B*u(k) */
            public Matrix state_post;          /* corrected state (x(k)):
                                        x(k)=x'(k)+K(k)*(z(k)-H*x'(k)) */
            public Matrix transition_matrix;   /* state transition matrix (A) */
            public Matrix control_matrix;      /* control matrix (B)
                                       (it is not used if there is no control)*/
            public Matrix measurement_matrix;  /* measurement matrix (H) */
            public Matrix process_noise_cov;   /* process noise covariance matrix (Q) */
            public Matrix measurement_noise_cov; /* measurement noise covariance matrix (R) */
            public Matrix error_cov_pre;       /* priori error estimate covariance matrix (P'(k)):
                                        P'(k)=A*P(k-1)*At + Q)*/
            Matrix gain;                /* Kalman gain matrix (K(k)):
                                        K(k)=P'(k)*Ht*inv(H*P'(k)*Ht+R)*/
            Matrix error_cov_post;      /* posteriori error estimate covariance matrix (P(k)):
                                        P(k)=(I-K(k)*H)*P'(k) */
            Matrix temp1;               /* temporary matrices */
            Matrix temp2;
            Matrix temp3;
            Matrix temp4;
            Matrix temp5;

            public KalmanFilter()
            {
                MP = 1;
                DP = 2;
                CP = 0;
                state_pre = new Matrix(DP, 1);
                state_pre.Zero();
                state_post = new Matrix(DP, 1);
                state_post.Zero();
                transition_matrix = new Matrix(DP, DP);
                transition_matrix.SetIdentity(1.0);
                transition_matrix[0, 1] = 1;
                process_noise_cov = new Matrix(DP, DP);
                process_noise_cov.SetIdentity(1.0);
                measurement_matrix = new Matrix(MP, DP);
                measurement_matrix.SetIdentity(1.0);
                measurement_noise_cov = new Matrix(MP, MP);
                measurement_noise_cov.SetIdentity(1.0);
                error_cov_pre = new Matrix(DP, DP);
                error_cov_post = new Matrix(DP, DP);
                error_cov_post.SetIdentity(1);
                gain = new Matrix(DP, MP);
                if (CP > 0)
                {
                    control_matrix = new Matrix(DP, CP);
                    control_matrix.Zero();
                }
                //
                temp1 = new Matrix(DP, DP);
                temp2 = new Matrix(MP, DP);
                temp3 = new Matrix(MP, MP);
                temp4 = new Matrix(MP, DP);
                temp5 = new Matrix(MP, 1);
            }

            public Matrix Predict()
            {
                state_pre = transition_matrix.Multiply(state_post);
                //if (CP>0)
                //{
                //    control_matrix
                //}
                temp1 = transition_matrix.Multiply(error_cov_post);
                Matrix at = transition_matrix.Transpose();
                error_cov_pre = temp1.Multiply(at).Add(process_noise_cov);

                Matrix result = new Matrix(state_pre);
                return result;
            }

            public Matrix Correct(Matrix measurement)
            {
                temp2 = measurement_matrix.Multiply(error_cov_pre);
                Matrix ht = measurement_matrix.Transpose();
                temp3 = temp2.Multiply(ht).Add(measurement_noise_cov);
                temp3.InvertSsgj();
                temp4 = temp3.Multiply(temp2);
                gain = temp4.Transpose();

                temp5 = measurement.Subtract(measurement_matrix.Multiply(state_pre));
                state_post = gain.Multiply(temp5).Add(state_pre);

                error_cov_post = error_cov_pre.Subtract(gain.Multiply(temp2));

                Matrix result = new Matrix(state_post);
                return result;
            }

            public Matrix AutoPredict(Matrix measurement)
            {
                Matrix result = Predict();

                Correct(measurement);

                return result;
            }
        }
        #endregion

        public KalmanFacade(int valueItem)
        {
            if (valueItem<=0)
            {
                throw new Exception("not enough value items!");
            }
            kmfilter = new KalmanFilter[valueItem];
            Random rand = new Random(1001);
            for (int i = 0; i < valueItem; i++ )
            {
                kmfilter[i] = new KalmanFilter();
                kmfilter[i].state_post[0, 0] = rand.Next(100)*0.001;
                kmfilter[i].state_post[1, 0] = rand.Next(100)*0.001;
                //
                kmfilter[i].process_noise_cov.SetIdentity(10);
                kmfilter[i].measurement_noise_cov.SetIdentity(10);
            }
        }

        private KalmanFilter[] kmfilter = null; 
        
        public bool GetValue(double[] inValue, ref double[] outValue)
        {
            if (inValue.Length != kmfilter.Length || outValue.Length != kmfilter.Length)
            {
                return false;
            }

            Matrix[] measures = new Matrix[kmfilter.Length];
            
            for (int i = 0; i < kmfilter.Length; i++ )
            {
                measures[i] = new Matrix();
                measures[i][0, 0] = inValue[i];
                outValue[i] = kmfilter[i].AutoPredict(measures[i])[0, 0];
            }

            return true;
        }
    }
}
