using System;
using System.Collections.Generic;
//dotnet run --property WarningLevel=0



class markov{



    public int[,,,] tiles;
    double[] weights;
    double[,,] wsums;
    double[] wlogs;
    double[,,] wlogssums;
    double[,,] entropy;
    int[,] stack;

    int[,,] chain;
    int[,] clen;
    int[] cx,cy,cz;


    public int tx,ty,tz,tt,sx,sy,sz;
    public bool[,,,] wave;
    int[,,] lens;
    int ndone;
    int nstack;
    Random rand;
    

    public markov(int[,,] example, int Tx,int Ty,int Tz,int Sx,int Sy,int Sz){
        int[,,,] Tiles;
        double[] Weights;
        int Tt=0;

        example2tile(example, ref Tiles, ref Weights, ref Tt,Tx,Ty,Tz);
        setup(Tiles,Weights,Tx,Ty,Tz,Tt,Sx,Sy,Sz);

    }
    public markov(int[,,,] Tiles, double[] Weights,int Tx,int Ty,int Tz,int Tt,int Sx,int Sy,int Sz){
        setup(Tiles,Weights,Tx,Ty,Tz,Tt,Sx,Sy,Sz);
    }
    void setup(int[,,,] Tiles, double[] Weights,int Tx,int Ty,int Tz,int Tt,int Sx,int Sy,int Sz){
        tiles=Tiles;
        weights=Weights;
        tx=Tx;ty=Ty;tz=Tz;tt=Tt;sx=Sx;sy=Sy;sz=Sz;
        rand=new Random();
        
        
    }

    public void run(){
        int x,y,z,t;
        clear();
        conns();
    }

    void conns(){
        int x,y,z;
        for(int i=0;i<tt;i++)
        for(int j=0;j<6;j++) {
            x=ix(-cx[j],tx);
            y=ix(-cy[j],ty);
            z=ix(-cz[j],tz);
            if (tiles[x,y,z,i]==-1){
                chain[tt,j,clen[tt,j]]=i;
                clen[tt,j]++;
            }
          
            for(int k=0;k<tt;k++)
            if (overlap(i,k,cx[j],cx[j],cx[j])){
                chain[i,j,clen[i,j]]=k;
                clen[i,j]++;
            }
        }
    }
    bool overlap(int idx1, int idx2, int dx, int dy, int dz){
        for(int x=0;x<tx;x++)
        for(int y=0;y<ty;y++)
        for(int z=0;z<tz;z++){
            int x0,y0,z0;
            x0=x+dx; y0=y+dy; z0=z+dz;
            if(x0>=0 && x0<tx && y0>=0 && y0<ty && z0>=0 && z0<tz)
                if(tiles[x0,y0,z0,idx2] != tiles[x,y,z,idx1])
                    return false;
            
        }
        return true;
    }
    void clear(){
        wave = new bool[sx,sy,sz,tt];
        lens = new int[sx,sy,sz];
        wsums= new double[sx,sy,sz];
        wlogs= new double[tt];
        wlogssums= new double[sx,sy,sz];
        entropy= new double[sx,sy,sz] ;
        stack=new int[sx*sy*sz,4];
        ndone=0;
        nstack=0;
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
        chain=new int[tt+1,6,tt+1];
        clen =new int[tt+1,6];
        cx=new int[6]{1,-1,0,0,0,0};
        cy=new int[6]{0,0,1,-1,0,0};
        cz=new int[6]{0,0,0,0,1,-1};
        for(int i=0;i<tt+1;i++)
        for(int j=0;j<6;j++){
            clen[i,j]=0;
            for(int k=0;k<tt+1;k++)
                chain[i,j,k]=-1;
        }
            
        
    }



    public static void example2tile(int[,,] example, ref int[,,,] Tiles, ref double[] Weights, ref int Tt, int Tx, int Ty, int Tz){
        List<int[,,]> tiles = new List<int[,,]>();
        List<double> w=new List<double>();
        double summ=0;
        int Sx=example.GetLength(0);
        int Sy=example.GetLength(1);
        int Sz=example.GetLength(2);
        for (int x=0;x<Sx;x++)
        for (int y=0;y<Sy;y++)
        for (int z=0;z<Sz;z++){
            bool is_negative=false;
            int[,,] tile = new int[Tx,Ty,Tz];
            for (int dx=0;dx<Tx;dx++)
            for (int dy=0;dy<Ty;dy++)
            for (int dz=0;dz<Tz;dz++){
                int X=ix(x+dx,Tx);
                int Y=ix(y+dy,Ty);
                int Z=ix(z+dz,Tz);
                if ((X<0 || X>=Sx || Y<0 || Y>=Sy || Z<0 || Z>=Sz)){
                    tile[dx,dy,dz]=-1;
                    is_negative=true;
                }
                else
                    tile[dx,dy,dz]=example[X,Y,Z];
            }
            if(!is_negative){
                int c = check(tile,tiles);
                summ+=1;
                if(c<0){
                    tiles.Add(tile);
                    w.Add(1);
                }
                else{
                    w[c]+=1;
                }
            }
        }
        Tt=w.Count;
        Weights=new double[Tt];
        Tiles=new int[Tx,Ty,Tz,Tt];
        for (int t=0;t<Tt;t++){
            Weights[t]=w[t];///summ;
        
            for (int x=0;x<Tx;x++)
            for (int y=0;y<Ty;y++)
            for (int z=0;z<Tz;z++)
                Tiles[x,y,z,t]=tiles[t][x,y,z];
        }
    }

    public static int check(int[,,] tile,List<int[,,]> tiles){
        int Sx=tile.GetLength(0);
        int Sy=tile.GetLength(1);
        int Sz=tile.GetLength(2);

        int t=tiles.Count;
        for(int i=0;i<t;i++){
            bool is_same=true;
            for (int x=0;x<Sx;x++)
            for (int y=0;y<Sy;y++)
            for (int z=0;z<Sz;z++)
            if(tiles[i][x,y,z]!=tile[x,y,z])
                is_same=false;
            if(is_same)
                return i;
        }
        return -1;
    }

    public static int ix(int idx,int idx_size){
        return idx-idx_size/2+1-idx_size%2;
    }
    


}

