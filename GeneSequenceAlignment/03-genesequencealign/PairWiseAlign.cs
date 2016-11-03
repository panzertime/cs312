using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticsLab
{
    class PairWiseAlign
    {
        int MaxCharactersToAlign;

        readonly int SUB_SCORE = 1;
        readonly int INDEL_SCORE = 5;
        readonly int MATCH_SCORE = -3;
        // we seek the MINIMAL final score

        readonly int LEFT = 3;
        readonly int UP = 1;
        readonly int DIAG = 4;

        public PairWiseAlign()
        {
            // Default is to align only 5000 characters in each sequence.
            this.MaxCharactersToAlign = 5000;
        }

        public PairWiseAlign(int len)
        {
            // Alternatively, we can use an different length; typically used with the banded option checked.
            this.MaxCharactersToAlign = len;
        }

        /// <summary>
        /// this is the function you implement.
        /// </summary>
        /// <param name="sequenceA">the first sequence</param>
        /// <param name="sequenceB">the second sequence, may have length not equal to the length of the first seq.</param>
        /// <param name="banded">true if alignment should be band limited.</param>
        /// <returns>the alignment score and the alignment (in a Result object) for sequenceA and sequenceB.  The calling function places the result in the dispay appropriately.
        /// 
        public ResultTable.Result Align_And_Extract(GeneSequence sequenceA, GeneSequence sequenceB, bool banded)
        {
            
            int score;                                                       // place your computed alignment score here
            string[] alignment = new string[2];                              // place your two computed alignments here



            int X_dim = sequenceA.Sequence.Length > MaxCharactersToAlign ? MaxCharactersToAlign : sequenceA.Sequence.Length;
            int Y_dim = sequenceB.Sequence.Length > MaxCharactersToAlign ? MaxCharactersToAlign : sequenceB.Sequence.Length;


            int[,,] NW_Result;

            if (banded)
            {
                NW_Result = NW_Banded(sequenceA, sequenceB, X_dim, Y_dim);
            }
            else
            {
                NW_Result = NW_Unbanded(sequenceA, sequenceB, X_dim, Y_dim);
            }
            string[] NW_Alignment = NW_Extract(NW_Result, X_dim, Y_dim, sequenceA, sequenceB);

            // ********* these are placeholder assignments that you'll replace with your code  *******
            score = NW_Result[X_dim, Y_dim, 0];                                                
            alignment[0] = NW_Alignment[0];
            alignment[1] = NW_Alignment[1];
            // ***************************************************************************************

            ResultTable.Result result = new ResultTable.Result();
            result.Update(score,alignment[0],alignment[1]);                  // bundling your results into the right object type 
            return(result);
        }

        private int[,,] NW_Unbanded(GeneSequence sequenceA, GeneSequence sequenceB, int X_dim, int Y_dim)
        {
            char[] A = sequenceA.Sequence.ToCharArray();
            char[] B = sequenceB.Sequence.ToCharArray();

            int[,,] result = new int[X_dim + 1, Y_dim + 1, 2];

            // we now have a 3-D array.  
            //  result[x][y][0] is the score
            //  result[x][y][1] is the alignment backpointer
            //      1 is up, 3 is left, 4 is diagonal
            //      (think Cartesian plane quadrants)

            // now, populate the first row and column:
            result[0, 0, 0] = 0;
            result[0, 0, 1] = 4;
            for (int i = 1; i <= X_dim; i++)
            {
                result[i, 0, 0] = result[i - 1, 0, 0] + INDEL_SCORE;
                result[i, 0, 1] = LEFT;
            }
            for (int i = 1; i <= Y_dim; i++)
            {
                result[0, i, 0] = result[0, i - 1, 0] + INDEL_SCORE;
                result[0, i, 1] = UP;
            }

            // now, begin the Needleman-Wunsch algo proper:
            for (int x = 1; x <= X_dim; x++)
            {
                for (int y = 1; y <= Y_dim; y++)
                {
                    int end_score;
                    int end_align;

                    end_score = result[x - 1, y, 0] + INDEL_SCORE;
                    end_align = LEFT;
                    if (end_score > result[x, y - 1, 0] + INDEL_SCORE)
                    {
                        end_score = result[x, y - 1, 0] + INDEL_SCORE;
                        end_align = UP;
                    }
                    // not an else, must try all options
                    if (A[x - 1] == B[y - 1]) 
                    {
                        // check match score
                        if (end_score > result[x - 1, y - 1, 0] + MATCH_SCORE)
                        {
                            end_score = result[x - 1, y - 1, 0] + MATCH_SCORE;
                            end_align = DIAG;
                        }
                    }
                    else
                    {
                        //check substitution score
                        if (end_score > result[x - 1, y - 1, 0] + SUB_SCORE)
                        {
                            end_score = result[x - 1, y - 1, 0] + SUB_SCORE;
                            end_align = DIAG;
                        }
                    }

                    result[x, y, 0] = end_score;
                    result[x, y, 1] = end_align;
                }
            }

            return result;
        }

        private int[,,] NW_Banded(GeneSequence sequenceA, GeneSequence sequenceB, int X_dim, int Y_dim)
        {
            char[] A = sequenceA.Sequence.ToCharArray();
            char[] B = sequenceB.Sequence.ToCharArray();

            int[,,] result = new int[X_dim + 1, Y_dim + 1, 2];

            // we now have a 3-D array.  
            //  result[x][y][0] is the score
            //  result[x][y][1] is the alignment backpointer
            //      1 is up, 3 is left, 4 is diagonal
            //      (think Cartesian plane quadrants)

            // now, populate the table with max_value, as well as INDEL vals in first row and col
            for (int x = 0; x <= X_dim; x++)
            {
                for (int y = 0; y <= Y_dim; y++)
                {
                    result[x, y, 0] = Int32.MaxValue - 100;
                }
            }
            result[0, 0, 0] = 0;
            result[0, 0, 1] = 4;
            for (int i = 1; i <= 3; i++)
            {
                result[i, 0, 0] = result[i - 1, 0, 0] + INDEL_SCORE;
                result[i, 0, 1] = LEFT;
            }
            for (int i = 1; i <= 3; i++)
            {
                result[0, i, 0] = result[0, i - 1, 0] + INDEL_SCORE;
                result[0, i, 1] = UP;
            }

            // since Needleman-Wunsch can't be banded with strings differing more than the bandwidth in length
            // just skip the whole thing to save time then
            if (X_dim - Y_dim > 3 || X_dim - Y_dim < -3)
            {
                return result;
            }

            // now, begin the Needleman-Wunsch algo proper:
            for (int y = 1; y <= Y_dim; y++)
            {
                for (int x = (y - 3 < 1 ? 1 : y - 3); x <= y + 3 && x <= X_dim; x++)
                {

                    int end_score;
                    int end_align;

                    end_score = result[x - 1, y, 0] + INDEL_SCORE;
                    end_align = LEFT;
                    if (end_score > result[x, y - 1, 0] + INDEL_SCORE)
                    {
                        end_score = result[x, y - 1, 0] + INDEL_SCORE;
                        end_align = UP;
                    }
                    // not an else, must try all options
                    if (A[x - 1] == B[y - 1])
                    {
                        // check match score
                        if (end_score > result[x - 1, y - 1, 0] + MATCH_SCORE)
                        {
                            end_score = result[x - 1, y - 1, 0] + MATCH_SCORE;
                            end_align = DIAG;
                        }
                    }
                    else
                    {
                        //check substitution score
                        if (end_score > result[x - 1, y - 1, 0] + SUB_SCORE)
                        {
                            end_score = result[x - 1, y - 1, 0] + SUB_SCORE;
                            end_align = DIAG;
                        }
                    }

                    result[x, y, 0] = end_score;
                    result[x, y, 1] = end_align;
                }
            }

            return result;
        }

        private string[] NW_Extract(int[,,] table, int X_dim, int Y_dim, GeneSequence sequenceA, GeneSequence sequenceB)
        {
            StringBuilder A_build = new StringBuilder();
            StringBuilder B_build = new StringBuilder();

            if (table[X_dim, Y_dim, 0] == Int32.MaxValue - 100)
            {
                return new String[] { "No Alignment Possible", "No Alignment Possible" };
            }

            string A_temp = sequenceA.Sequence;
            string B_temp = sequenceB.Sequence;

            char[] A = A_temp.Insert(0, "-").ToCharArray();
            char[] B = B_temp.Insert(0, "-").ToCharArray();

            int x = X_dim;
            int y = Y_dim;

            while (x > -1 && y > -1)
            {
                if (x == 0 && y == 0)
                {
                    // we kinda designed around this but let's just quit to be safe
                    break;
                }

                if (table[x, y, 1] == DIAG)
                {
                    // just add each character to the outputs
                    A_build.Insert(0, A[x]);
                    B_build.Insert(0, B[y]);
                    // now climb diagonally
                    x--;
                    y--;
                }
                else if (table[x, y, 1] == UP)
                {
                    // an indel.  
                    A_build.Insert(0, '-');
                    B_build.Insert(0, B[y]);
                    y--;
                }
                else if (table[x, y, 1] == LEFT)
                {
                    // an indel.  
                    A_build.Insert(0, A[x]);
                    B_build.Insert(0, '-');
                    x--;
                }
            }


            return new String[] {A_build.ToString(), B_build.ToString()} ;
        }
    }
}
