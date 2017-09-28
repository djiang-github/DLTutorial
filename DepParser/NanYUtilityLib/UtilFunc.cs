using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    static public class UtilFunc
    {
        public static void SortAgainstScore<T>(T[] items, float[] scores)
        {
            SortHelper<T>(items, scores, 0, scores.Length - 1);
        }

        public static void SortAgainstScore<T>(T[] items, float[] scores, int count)
        {
            SortHelper<T>(items, scores, 0, count - 1);
        }

        private static void SortHelper<T>(T[] items, float[] score, int left, int right)
        {
            float pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                SortHelper<T>(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                SortHelper<T>(items, score, left + 1, r_hold);
            }
        }

        //int editDis( const wchar_t * source,  wchar_t * target )
        //{
        //    wchar_t noSpace[1000];
        //    if( wcschr( target, ' ' ) != NULL )
        //        removeSpace( target, noSpace );

        //    target = noSpace;
        //    //wprintf(L"%s\n", target);
	
        //    int sourceLen = wcslen(source);
        //    int targetLen = wcslen(target);
        //    /* allocate space */
        //    int dis[500][500];
        //    /* init dis */
        //    for( int i = 0; i <= sourceLen ; ++i )
        //        dis[ i ][0] = i;
        //    for( int i = 0; i <= targetLen; ++i )
        //        dis[0][i] = i;
        //    for( int i = 1; i < sourceLen + 1; ++i )
        //    {
        //        for( int k = 1; k < targetLen + 1; ++k )
        //        {
        //            int	cp_replace = target[k - 1] == source[i - 1]? cost[COPY]:cost[REPLACE];
        //            dis[ i ][ k ] = min( dis[i-1][k-1] + cp_replace, dis[ i - 1 ][ k ] + cost[ DELETE ] );
        //            dis[ i ][ k ] = min( dis[ i ][ k ], dis[ i ][k - 1 ] + cost[INSERT] );
        //        }
        //    }
        //    return dis[ sourceLen ][ targetLen ]; 
        //}

        public static int EditDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
            {
                return 0;
            }
            else if (string.IsNullOrEmpty(source))
            {
                return target.Length;
            }
            else if (string.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            int targetLen = target.Length;
            int sourceLen = source.Length;

            int[,] dis = new int[sourceLen + 1, targetLen + 1];

            const int COPY = 0;
            const int REPLACE = 1;
            const int DELETE = 1;
            const int INSERT = 1;

            for( int i = 0; i <= sourceLen ; ++i )
                dis[i,0] = i;
            for (int i = 0; i <= targetLen; ++i)
                dis[0,i] = i;
            for (int i = 1; i < sourceLen + 1; ++i)
            {
                for (int k = 1; k < targetLen + 1; ++k)
                {
                    int cp_replace = target[k - 1] == source[i - 1] ? COPY : REPLACE;
                    dis[i,k] = Math.Min(dis[i - 1,k - 1] + cp_replace, dis[i - 1,k] + DELETE);
                    dis[i,k] = Math.Min(dis[i,k], dis[i,k - 1] + INSERT);
                }
            }
            return dis[sourceLen,targetLen]; 
        }
    }
}
