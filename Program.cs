using System;


static class Program
{
    static void Main()
    {
        int X=0;
        int tx=2;
        for(int x=0; x<tx; x++){
            var q=wfc.ix(x,tx);
            System.Console.WriteLine(q);
            System.Console.WriteLine(-wfc.ix(-q,tx));
            System.Console.WriteLine(" ");
        }
            
        
      
        return;
    }
}
