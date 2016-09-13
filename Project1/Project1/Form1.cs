using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace Project1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            long k = 5; 
            if (! k_hole.Text.Contains("k"))
            {
                k = Convert.ToInt64(k_hole.Text);
            }
            if (prime_test(Convert.ToInt64(input.Text), k))
            {
                output.Text = input.Text + " is prime with k = " + k.ToString();
            }
            else
            {
                output.Text = input.Text + " is NOT prime with k = " + k.ToString();
            }
        }

        private bool prime_test(long N, long k)
        {
            ISet < long > a = new HashSet<long>();
            Random rand = new Random();

            for (int i = 0; i < k; i++)
            {
                long t = rand.Next((int) N);
                if (a.Contains(t))
                {
                    i--;
                }
                else
                {
                    a.Add(t);
                    if(modexp(t, N - 1, N) != 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        long modexp(long x, long y, long N)
        {
            if (y == 0)
            {
                return 1;
            }
            if (y % 2 == 0)
            {
                long z = modexp(x, y / 2, N);
                return  (z * z) % N;
            }
            else
            {
                long z = modexp(x, (y - 1) / 2, N);
                return (x * (z * z)) % N;
            }
        }
    }
}
