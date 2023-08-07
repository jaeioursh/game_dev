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

    bool[,,] chain;
    
    int[] cx,cy,cz;


    public int tx,ty,tz,tt,sx,sy,sz;
    public bool[,,,] wave;
    public int[,,] collapse;
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

    int[,,] void run(){
        
        clear();
        conns();
        return build();
    }

    int[,,] build(){
        int[,,] map = new int[sx,sy,sz];
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++)
            map[x,y,z]=-1;
        if (recur(0,ref map)){
            for(int x=0;x<sx;x++)
            for(int y=0;y<sy;y++)
            for(int z=0;z<sz;z++)
                map[x,y,z]=tiles[ix(0,tx),ix(0,ty),ix(0,tz),map[x,y,z]];
            return map;
        }
        else
            throw new InvalidOperationException("Impossible Geometry");
            
    }

    bool recur(int i,ref int [,,] map){
        if (i==sx*sy*sz)
            return true;
        int[] opts,temp_opts;
        int x0,y0,z0;
        double best_len,temp_len;
        best_len=1e9;
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++){
            if (map[x,y,z]<0){
                temp_opts=options(x,y,z,map);
                if(temp_opts.Length == 0)
                    return false;
                
                temp_len=(double)temp_opts.Length+rand.NextDouble()*1e-3;
                if (temp_len<best_len){
                    best_len=temp_len;
                    opts=temp_opts;
                    x0=x;y0=y;z0=z;
                }
            }
        }
        shuffle(opts);
        for(int j=0;j<opts.Length;j++){
            map[x0,y0,z0]=opts[j];
            if (recur(i+1, ref map))
                return true;
        }
        map[x0,y0,z0]=-1;
        return false;
    }

    public static void shuffle (int[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rand.Next(n--);
            int temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    int[] options(int x,int y,int z,int[,,] map){
        int dx,dy,dz,t2;
        List<int> opts=new List<int>();
        bool checker;
        for(int t=0;t<tt;t++){
            checker=true;
            for(int w=0;w<6;w++){
                dx=x-cx[w];dy=y-cy[w];dz=z-cz[w];
                t2=-1;
                if(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz)
                    t2=tt; 
                else
                    if(map[dx,dy,dz]>=0)
                        t2=map[dx,dy,dz];
                if (t2>=0)
                    checker = checker && chain[t,w,t2];                    
            }
            if(checker)
                opts.Add(t);
        }
        return opts.ToArray();
    }

    void conns(){
        int x,y,z;
        for(int i=0;i<tt;i++)
        for(int j=0;j<6;j++) {
            x=ix(-cx[j],tx);
            y=ix(-cy[j],ty);
            z=ix(-cz[j],tz);
            if (tiles[x,y,z,i]==-1){
                chain[tt,j,i]=true;
                
            }
          
            for(int k=0;k<tt;k++)
            if (overlap(i,k,cx[j],cx[j],cx[j])){
                chain[i,j,k]=true;
                
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
        collapse = new int[sx,sy,sz];
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
            collapse[x,y,z]=-1;
            wsums[x,y,z]=1.0;
            wlogssums[x,y,z]=logs;
            entropy[x,y,z]=logs;
            lens[x,y,z]=tt;
            for(int t=0;t<tt;t++)
                wave[x,y,z,t]=true;

        }
        chain=new bool[tt+1,6,tt+1];
        
        cx=new int[6]{1,-1,0,0,0,0};
        cy=new int[6]{0,0,1,-1,0,0};
        cz=new int[6]{0,0,0,0,1,-1};
        for(int i=0;i<tt+1;i++)
        for(int j=0;j<6;j++){
            for(int k=0;k<tt+1;k++)
                chain[i,j,k]=false;
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


    public static int ix(int idx,int idx_size){
        return idx-idx_size/2+1-idx_size%2;
    }
    


}

