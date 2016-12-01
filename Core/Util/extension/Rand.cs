using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     random num generator
    /// </summary>
    public class Rand
    {
        private static Rand _instance;
        private static Rand Instance => _instance ?? (_instance = new Rand());

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool NextBoolean() => Next(2) == 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int Next() => Instance.GetProb(32767);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int max) => Instance.GetProb(max);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int min, int max) => min + Instance.GetProb(max - min);

        /// <summary>
        ///     在总长为Sum(input)的线段中随机一个点
        ///     返回随机点所在的区间索引
        /// 
        ///     例如：
        ///         Next(1, 3, 6)
        ///         长度为10的线段，以1/3/6为长度连续划分为3段，亦即[0,1),[1,4),[4,10)
        ///     若Next(10)=5，此时5落在第3段，所以返回2
        ///     若Next(10)=1, 则返回0
        ///     若Next(10)=2, 则返回1
        ///     等...
        /// </summary>
        /// <param name="input"></param>
        public static int Next(params int[] input)
        {
            if (input.IsNullOrEmpty()) throw new ArgumentException("input");
            var len = input.Sum();
            var rd = Next(len);

            var tmpSum = 0;
            for (var i = 0; i < input.Length; i++)
            {
                tmpSum += input[i];
                if (rd < tmpSum)
                    return i;
            }

            throw new Exception("No possiable to go here!");
        }

        /// <summary>
        ///     [min, max)区间内随机cnt个不重数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public static List<int> Next(int min, int max, int cnt)
        {
            if (min > max) return null;

            var lst = new List<int>();
            if ((max - min) <= cnt)
            {
                for (var i = 0; i < cnt; i++)
                {
                    lst.Add(i + min);
                }

                return lst;
            }

            // 均分cnt段
            var step = (max - min) / cnt;
            var remain = (max - min) % cnt;
            for (var i = 0; i < cnt - 1; i++)
            {
                lst.Add(Next(i * step, (i + 1) * step));
            }
            lst.Add(Next(max - step - remain, max));

            return lst;
        }

        private int GetProb(int nMax)
        {
            if (nMax <= 0)
            {
                return 0;
            }

            var nRandNumber = WELLRNG512();
            //            unsigned int x = nRandNumber % 127773;//127773是个较大的质数
            //			unsigned int x = nRandNumber % 1000003;//1000003是个较大的质数,作此变更的原因的是因为策划的掉落概率需求要改成百万分之X
            //以后需求再有变更请到该网址选择一个相对合适的质数 
            //http://www.buiosch.edu.hk/subjects/maths/PimeGenerator/prime.html
            return (int)(nRandNumber % ((uint)nMax));//x * nMax /127773;
        }

        private Rand()
        {
            index = 0;
            //state[16] = {0, 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
            //for ( int i = 0 ; i < 16; i++)
            //{
            //    state[i] = i;
            //}

            ProphasePrepare();
        }

        /*static */

        private void ProphasePrepare()
        {
            initRNG();
            for (var i = 0; i < 100; i++)
            {
                WELLRNG512();
            }

            for (var i = 0; i < 16; i++)
            {
                state[i] = WELLRNG512();
            }
        }

        /*static */
        void initRNG()
        {
            for (var i = 0; i < 16; )
            {
                var uiT = (uint)new Random().Next(32767);
                if (uiT != 0)
                {
                    state[i] = uiT;
                    i++;
                }
            }
        }

        //返回32位随机数
        /*static */
        uint WELLRNG512()
        {
            uint a, b, c, d;
            a = state[index];
            c = state[(index + 13) & 15];
            b = a ^ c ^ (a << 16) ^ (c << 15);
            c = state[(index + 9) & 15];
            c ^= (c >> 11);
            a = state[index] = b ^ c;
            d = (uint)(a ^ ((a << 5) & 0xDA442D24UL));
            index = (index + 15) & 15;
            a = state[index];
            state[index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);
            return state[index];
        }

        //初始化状态到随即位
        /*static */
        private uint[] state = new uint[16];
        //初始化必须为0
        /* static */
        private uint index;
    }

}
