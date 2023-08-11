using System;
using System.Collections.Generic;
//dotnet run --property WarningLevel=0



class wfc{



    public int[,,,] tiles;
    double[] weights;
    double[,,] wsums;
    double[] wlogs;
    double[,,] wlogssums;
    double[,,] entropy;
    int[,] stack;

    public int tx,ty,tz,tt,sx,sy,sz;
    public bool[,,,] wave;
    int[,,] lens;
    int ndone;
    int nstack;
    Random rand;
    

    public wfc(int[,,] example, int Tx,int Ty,int Tz,int Sx,int Sy,int Sz){
        int[,,,] Tiles=null;
        double[] Weights=null;
        int Tt=0;

        example2tile(example, ref Tiles, ref Weights, ref Tt,Tx,Ty,Tz);
        setup(Tiles,Weights,Tx,Ty,Tz,Tt,Sx,Sy,Sz);

    }
    public wfc(int[,,,] Tiles, double[] Weights,int Tx,int Ty,int Tz,int Tt,int Sx,int Sy,int Sz){
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
        boundary();

        while(ndone<sx*sy*sz){
            while(nstack>0){
                nstack--;
                x=stack[nstack,0];
                y=stack[nstack,1];
                z=stack[nstack,2];
                t=stack[nstack,3];
                
                propagate(x,y,z,t);
            }
            if(ndone<sx*sy*sz)
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
        ndone++;
        //propagate(ax,ay,az,at);
        stack[nstack,0]=ax;
        stack[nstack,1]=ay;
        stack[nstack,2]=az;
        stack[nstack,3]=at;
        nstack++;
    }
    
    void propagate(int X, int Y, int Z, int T,bool bounds=false){
        int val,val2;
        if(bounds)
            val = -1;
        else
            val = tiles[-ix(0,tx),-ix(0,ty),-ix(0,tz),T];
        //not centered
        for(int x=0; x<tx; x++)
        for(int y=0; y<ty; y++)
        for(int z=0; z<tz; z++)
        for(int t=0;t<tt;t++){
            //centered
            
            int dx=X+ix(x,tx);
            int dy=Y+ix(y,ty);
            int dz=Z+ix(z,tz);
            if (!(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz ))
                //if tile value appears in other others n tile
                if(wave[dx,dy,dz,t]==false && tiles[tx-x-1,ty-y-1,tz-z-1,t]!=val){
                    //if(lens[dx,dy,dz]==1)
                    //    throw new InvalidOperationException("opps");
                    rem(dx,dy,dz,t);
                }
                //if this pattern conflicts with others
                
            if(!bounds){
                val2=tiles[x,y,z,T];

            }
            
            
        }
    }

    void rem(int x, int y, int z, int t){

        

        wave[x,y,z,t]=false;
        lens[x,y,z]-=1;
        
        wsums[x,y,z]-=weights[t];
        wlogssums[x,y,z]-=wlogs[t];
        entropy[x,y,z]=Math.Log(wsums[x,y,z]) - wlogssums[x,y,z] / wsums[x,y,z];
        if (lens[x,y,z]==1){
            ndone++;
            for(int dt=0;dt<tt;dt++){
                if(wave[x,y,z,dt]==true){
                    stack[nstack,0]=x;
                    stack[nstack,1]=y;
                    stack[nstack,2]=z;
                    stack[nstack,3]=dt;
                    nstack++;
                }
            }
        }
        
    }

    void boundary(){
        for(int x=0; x<tx+sx-tx%2; x++)
        for(int y=0; y<ty+sy-ty%2; y++)
        for(int z=0; z<tz+sz-tz%2; z++){
            int dx=ix(x,tx);
            int dy=ix(y,ty);
            int dz=ix(z,tz);
            if ((dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz))
                propagate(dx,dy,dz,0,true);
            else
                alt_propagate(dx,dy,dz);  
        }

    }

    void alt_propagate(int X,int Y,int Z){
        for(int t=0;t<tt;t++)
        for(int x=0; x<tx; x++)
        for(int y=0; y<ty; y++)
        for(int z=0; z<tz; z++)
        {
            //centered
            int dx=X+ix(x,tx);
            int dy=Y+ix(y,ty);
            int dz=Z+ix(z,tz);
            if (!(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz || wave[X,Y,Z,t]==false))
                if(tiles[x,y,z,t]==-1)
                    rem(X,Y,Z,t);
        }
    }

    public int[,,] result(){
        int[,,] map=new int[sx,sy,sz];
        for (int x=0; x<sx;x++)
        for (int y=0; y<sy;y++)
        for (int z=0; z<sz;z++){
            map[x,y,z]=-1;
            for(int t=0;t<tt;t++)
                if(wave[x,y,z,t])
                    map[x,y,z]= tiles[-ix(0,tx),-ix(0,ty),-ix(0,tz),t];
        }
        return map;
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
            int[,,] tile = new int[Tx,Ty,Tz];
            for (int dx=0;dx<Tx;dx++)
            for (int dy=0;dy<Ty;dy++)
            for (int dz=0;dz<Tz;dz++){
                int X=ix(x+dx,Tx);
                int Y=ix(y+dy,Ty);
                int Z=ix(z+dz,Tz);
                if ((X<0 || X>=Sx || Y<0 || Y>=Sy || Z<0 || Z>=Sz))
                    tile[dx,dy,dz]=-1;
                else
                    tile[dx,dy,dz]=example[X,Y,Z];
            }
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
        Tt=w.Count;
        Weights=new double[Tt];
        Tiles=new int[Tx,Ty,Tz,Tt];
        for (int t=0;t<Tt;t++){
            Weights[t]=w[t]/summ;
        
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

