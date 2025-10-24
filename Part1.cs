using System;
using System.Text;
class Task1{
	public string solution(string s){
		int n = s.Length;
		for (int i=0;i<n-1;i++){
			if(s[i]>s[i+1]){
				return s.Remove(i,1);
			}
		}
		return s.Remove(n-1,1);
	}
}
class Task3{
 	public int solution(int[]A){
		int n = A.Length;
		if(n<=1) return 0;
		Array.Sort(A);
		long res =0;
		for (int i=0;i<n;i++){
			res = res + Math.Abs(A[i]-i-1);
			if(res >=2147483647){
				Console.WriteLine("Ket qua vuot qua gia tri int ma task 3 da giao");
				return 0;
			}
		}
		return (int)res;
	}
}
class Task2{
	public int solution(int [][]A){
		int n = A.Length;
		int m = A[0].Length;
		int res=-1;
		int [] max1 = new int[n];
		int [] col1 = new int[n];
		int [] max2 = new int[n];
		int [] col2 = new int[n];
		for (int i=0;i<n;i++){
			max1[i]=-1;
			col1[i]=-1;
			max2[i]=-1;
			col2[i]=-1;
			for(int j=0;j<m;j++){
				int temp = A[i][j];
				if(temp >=max1[i]){
					max2[i]=max1[i];
					col2[i]=col1[i];
					col1[i]=j;
					max1[i] = temp;
				}
				else if(temp >= max2[i]){
					max2[i]=temp;
					col2[i]=j;
				}
			}
		}
		for(int i=0;i<n-1;i++){
			for(int j=i+1;j<n;j++){
				int i_max1= max1[i];
				int i_max2= max2[i];
				int col1_i = col1[i];
				int col2_i = col2[i];
				
				int j_max1= max1[j];
				int j_max2= max2[j];
				int col1_j = col1[j];
				int col2_j = col2[j];
				if(col1_i!=col1_j){
					res = Math.Max(res,i_max1+j_max1);
				}
				else{
					res = Math.Max(res,Math.Max(i_max1+j_max2,i_max2+j_max1));
				}
				
			}
		}
		return res;
	}
}
public class Task{
	public static void Main(String[] afaf){
		Task1 solve1 = new Task1();
		Task3 solve3 = new Task3();
		Task2 solve2 = new Task2();
		int t = int.Parse(Console.ReadLine());
		while(t-- >0){
			/* Task1 
			string s = Console.ReadLine();
			Console.WriteLine(solve1.solution(s));
			*/
			/* Task 3
			int n = int.Parse(Console.ReadLine());
			int [] A = new int [n];
			string s = Console.ReadLine();
			string [] nums = s.Split(' ');		
			for (int i=0;i<n;i++){
				A[i] = int.Parse(nums[i]);
			}
			int res = solve3.solution(A);
			Console.WriteLine(res);
			*/
			string [] temp = Console.ReadLine().Split(' ');
			int n = int.Parse(temp[0]);
			int m = int.Parse(temp[1]);
			int [][] A = new int[n][];
			string [][] nums = new string[n][];
			for(int i=0;i<n;i++){
				nums[i] = Console.ReadLine().Split(' ');
			}
			for (int i=0;i<n;i++){
				A[i] = new int[m];
			}
			for (int i=0;i<n;i++){
				for (int j=0;j<m;j++){
					A[i][j] = int.Parse(nums[i][j]);
				}
			}
			Console.WriteLine("AA");
			int res = solve2.solution(A);
			Console.WriteLine(res);
		}
	}
}