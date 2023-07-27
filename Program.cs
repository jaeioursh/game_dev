using System;
using System.Collections.Generic;
//dotnet run --property WarningLevel=0



class wfc{



    int[,,,] tiles;
    double[] weights;
    double[,,] wsums;
    double[] wlogs;
    double[,,] wlogssums;
    double[,,] entropy;


    int tx,ty,tz,tt,sx,sy,sz;
    bool[,,,] wave;
    int[,,] lens;
    Random rand;
    

    wfc(int[,,] example, int Tx,int Ty,int Tz,int Sx,int Sy,int Sz){
        int[,,,] Tiles=null;
        double[] Weights=null;
        int Tt=0;

        example2tile(example, ref Tiles, ref Weights, ref Tt,Tx,Ty,Tz);
        setup(Tiles,Weights,Tx,Ty,Tz,Tt,Sx,Sy,Sz);

    }
    wfc(int[,,,] Tiles, double[] Weights,int Tx,int Ty,int Tz,int Tt,int Sx,int Sy,int Sz){
        setup(Tiles,Weights,Tx,Ty,Tz,Tt,Sx,Sy,Sz);
    }
    void setup(int[,,,] Tiles, double[] Weights,int Tx,int Ty,int Tz,int Tt,int Sx,int Sy,int Sz){
        tiles=Tiles;
        weights=Weights;
        tx=Tx;ty=Ty;tz=Tz;tt=Tt;sx=Sx;sy=Sy;sz=Sz;
        rand=new Random();
        
        clear();
        boundary();
        for(int i=0; i<sx*sy*sz;i++){
            observe();
        }
    }

    void clear(){
        wave = new bool[sx,sy,sz,tt];
        lens = new int[sx,sy,sz];
        wsums= new double[sx,sy,sz];
        wlogs= new double[tt];
        wlogssums= new double[sx,sy,sz];
        entropy= new double[sx,sy,sz] ;

        double logs=0;
        for(int i=0;i<tt;i++){
            wlogs[i]=weights[i] * Math.Log(weights[i]);
            logs+=wlogs[i];
        }

        
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++){
            wsums[x,y,z]=1.0;
            wlogssums[x,y,z]=logs;
            entropy[x,y,z]=logs;
            lens[x,y,z]=tt;
            for(int t=0;t<tt;t++)
                wave[x,y,z,t]=true;

        }
        
        
    }

    void observe(){
        double min = 1E+4;
        int ax,ay,az,at;
        ax=ay=az=at=-1;
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++)
            
            if (lens[x,y,z] > 1)
            {
                double noise = 1E-6 * rand.NextDouble();
                if (entropy[x,y,z] + noise < min)
                {
                    min = entropy[x,y,z] + noise;
                    ax=x;ay=y;az=z;
                }
            }
        min=1e4;
        for(int t=0;t<tt;t++)
            
            if (wave[ax,ay,az,t]){
                double noise = rand.NextDouble();
                if (noise < min)
                {
                    min =  noise;
                    at=t;
                }
                wave[ax,ay,az,t]=false;
            }
        wave[ax,ay,az,at]=true;
        lens[ax,ay,az]=1;
        propagate(ax,ay,az,at);
    }
    
    void propagate(int X, int Y, int Z, int T,bool bounds=false){
        int val;
        if(bounds)
            val = -1;
        else
            val = tiles[-ix(0,tx),-ix(0,ty),-ix(0,tz),T];
        for(int x=0; x<tx; x++)
        for(int y=0; y<ty; y++)
        for(int z=0; z<tz; z++)
        for(int t=0;t<tt;t++){
            int dx=X-ix(x,tx);
            int dy=Y-ix(y,ty);
            int dz=Z-ix(z,tz);
            if (!(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz || wave[dx,dy,dz,t]==false))
                if(tiles[x,y,z,t]!=val)
                    rem(dx,dy,dz,t);
        }
    }

    void rem(int x, int y, int z, int t){
        wave[x,y,z,t]=false;
        lens[x,y,z]-=1;
        wsums[x,y,z]-=weights[t];
        wlogssums[x,y,z]-=wlogs[t];
        entropy[x,y,z]=Math.Log(wsums[x,y,z]) - wlogssums[x,y,z] / wsums[x,y,z];

    }

    void boundary(){
        for(int x=0; x<tx+sx; x++)
        for(int y=0; y<ty+sy; y++)
        for(int z=0; z<tz+sz; z++){
            int dx=x+ix(x,tx);
            int dy=y+ix(y,ty);
            int dz=z+ix(z,tz);
            if ((dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz))
                propagate(dx,dy,dz,0,true);
                
        }

    }

    public static void example2tile(int[,,] example, ref int[,,,] Tiles, ref double[] Weights, ref int Tt, int Tx, int Ty, int Tz){
        List<int[,,]> tiles = new List<int[,,]>();
        List<double> w=new List<double>();
        double summ=0;
        int Sx=example.GetLength(0);
        int Sy=example.GetLength(1);
        int Sz=example.GetLength(2);
        for int(x=0;x<Sx;x++)
        for int(y=0;y<Sy;y++)
        for int(z=0;z<Sz;z++){
            int[,,] tile = new int[Tx,Ty,Tz];
            for int(dx=0;dx<Tx;dx++)
            for int(dy=0;dy<Ty;dy++)
            for int(dz=0;dz<Tz;dz++){
                int X=ix(x+dx,Tx);
                int Y=ix(y+dy,Ty);
                int Z=ix(z+dz,Tz);
                if ((X<0 || X>=Sx || Y<0 || Y>=Sy || Z<0 || Z>=Sz))
                    tile[dx,dy,dz]=-1;
                else
                    tile[dx,dy,dz]=example[X,Y,Z];
            }
            int c = check(tile,tiles);
        }

    }

    public static void

    public static int ix(int idx,int idx_size){
        return idx-idx_size/2+1-idx_size%2;
    }
    


}



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
