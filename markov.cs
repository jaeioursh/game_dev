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
    bool verbose;
    Random rand;

    List<List<List<int>>> CCC;
    

    public markov(int[,,] example, int Tx,int Ty,int Tz,int Sx,int Sy,int Sz,bool rotatable = true){
        int[,,,] Tiles=null;
        double[] Weights=null;
        verbose=true;
        int Tt=0;

        example2tile(example, ref Tiles, ref Weights, ref Tt,Tx,Ty,Tz,rotatable);
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
        clear();
        conns();
        
    }

    public int[,,] run(){
        
        
        clearer();
        return build();
    }

    public void status(){
        if(verbose){
            Console.Write(ndone);
            Console.Write(" / ");
            Console.WriteLine(sx*sy*sz);
        }
    }
    public int[,,] build(){
        bool flag;
        bounds();
        fill_out();
        int[,,] map = new int[sx,sy,sz];
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++){
            flag=true;
            for(int t=0;t<tt;t++){
                
                if(wave[x,y,z,t]){
                    map[x,y,z]=tiles[-ix(0,tx),-ix(0,ty),-ix(0,tz),t];
                    //map[x,y,z]=t;
                    flag=false;
                }
            }
            if (flag)
                return null;
        }
        return map;
    }
    int[,,] build2(int limit,ref int depth){
        int[,,] map = new int[sx,sy,sz];
        int LIMIT=limit;
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++)
            map[x,y,z]=-1;
        if (recur(0,ref map, ref LIMIT, ref depth)){
            for(int x=0;x<sx;x++)
            for(int y=0;y<sy;y++)
            for(int z=0;z<sz;z++)
                map[x,y,z]=tiles[-ix(0,tx),-ix(0,ty),-ix(0,tz),map[x,y,z]];
            return map;
        }
        return null;
            
    }

    public int errs(int[,,] map){
        int err=0;
        int t,t2;
        int dx,dy,dz;
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++){
            t=map[x,y,z];
            
                
            for(int w=0;w<6;w++){
                dx=x-cx[w];dy=y-cy[w];dz=z-cz[w];
                if(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz)
                    t2=tt; 
                else
                    t2=map[dx,dy,dz];
                if (!chain[t2,w,t])
                    err++;                   
            }
            
        
        }
        return err;
    
    }
    
    void fill_out(){

        double min = 1E+4;
        int ax,ay,az,at;
        ax=ay=az=at=-1;
        while(ndone<sx*sy*sz){
        min=1e4;
        //randomly choose x,y,z
        for(int x=0;x<sx;x++)
        for(int y=0;y<sy;y++)
        for(int z=0;z<sz;z++)
            
            if (lens[x,y,z] > 1)
            {
                double noise = 1E-6 * rand.NextDouble();
                if ((double)lens[x,y,z] + noise < min)
                {
                    min = (double)lens[x,y,z] + noise;
                    ax=x;ay=y;az=z;
                }
            }
        min=1e4;
        //randomly choose t
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
        status();
        prop(ax,ay,az);

        }
    }
    void bounds(){
        //prop(0,-1,0);
        for(int x=-1;x<sx+1;x++)
        for(int y=-1;y<sy+1;y++)
        for(int z=-1;z<sz+1;z++)
        if(x<0 || x>=sx || y<0 || y>=sy || z<0 || z>=sz)
            prop(x,y,z);


        
    }
    void prop(int xx,int yy, int zz){
        Stack<int[]> stk = new Stack<int[]>();
        int[] curr;
        int x,y,z,dx,dy,dz,start,stop;
        bool flag;
        stk.Push(new int[] {xx,yy,zz});
        while(stk.Count>0){
            curr=stk.Pop();
            x=curr[0];y=curr[1];z=curr[2];
            
            if(x<0 || x>=sx || y<0 || y>=sy || z<0 || z>=sz){
                start=tt;
                stop=start+1;
            }
            else{
                start=0;
                stop=tt;
            }
            
            for(int w=0;w<6;w++){
                dx=x+cx[w];dy=y+cy[w];dz=z+cz[w];
                bool delta=false;
                if(!(dx<0 || dx>=sx || dy<0 || dy>=sy || dz<0 || dz>=sz)){
                    for(int j=0;j<tt;j++){
                        flag=false;
                        for(int i=start;i<stop;i++){
                            if(i==tt || wave[x,y,z,i]) //if -1 or potential tile
                                flag|=chain[i,w,j];
                        }
                        if (!flag){ //if not valid conn
                            if(wave[dx,dy,dz,j]){ // if previously valid tile
                                wave[dx,dy,dz,j]=false; //remove tile
                                delta=true;             //change occured and must propagate
                                lens[dx,dy,dz]--;
                                if(lens[dx,dy,dz]==1){
                                    ndone++;
                                    status();
                                }
                            }

                        }
                        
                    }
                if(delta)
                    stk.Push(new int[] {dx,dy,dz});    
                }
            }
        }

    }

    bool recur(int i,ref int [,,] map, ref int limit, ref int depth){
        limit-=1;
        if (i>depth)
            depth=i;            
        if (limit<0)
            return false;
        if (i==sx*sy*sz)
            return true;
        int[] opts,temp_opts;
        int x0,y0,z0;
        opts=null;
        x0=y0=z0=-1;
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
            if (recur(i+1, ref map,ref limit, ref depth))
                return true;
        }
        map[x0,y0,z0]=-1;
        return false;
    }

    void shuffle (int[] array)
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
                    checker = checker && chain[t2,w,t];                    
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
            x=-cx[j]-ix(0,tx);
            y=-cy[j]-ix(0,ty);
            z=-cz[j]-ix(0,tz);
            if (tiles[x,y,z,i]==-77){
                chain[tt,j,i]=true;
                
            }
          
            for(int k=0;k<tt;k++)
            if (overlap(i,k,cx[j],cy[j],cz[j])){
                chain[i,j,k]=true;
                
            }
        }
        view_chain();
    }

    void view_chain(){

        CCC = new List<List<List<int>>>();
        for(int w=0;w<6;w++){
            List<List<int>>  C = new List<List<int>>();
            for(int i=0;i<=tt;i++){
                List<int> CC = new List<int>();
                for(int j=0;j<=tt;j++)
                    if(chain[i,w,j])
                        CC.Add(j);
                C.Add(CC);
            }
            CCC.Add(C);
        }
        return;
        
    }
    
    bool overlap(int idx1, int idx2, int dx, int dy, int dz){
        for(int x=0;x<tx;x++)
        for(int y=0;y<ty;y++)
        for(int z=0;z<tz;z++){
            int x0,y0,z0;
            x0=x+dx; y0=y+dy; z0=z+dz;
            if(x0>=0 && x0<tx && y0>=0 && y0<ty && z0>=0 && z0<tz)
                if(tiles[x0,y0,z0,idx1] != tiles[x,y,z,idx2])
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
        
        chain=new bool[tt+1,6,tt+1];
        
        cx=new int[6]{1,-1,0,0,0,0};
        cy=new int[6]{0,0,1,-1,0,0};
        cz=new int[6]{0,0,0,0,1,-1};
        for(int i=0;i<tt+1;i++)
        for(int j=0;j<6;j++){
            for(int k=0;k<tt+1;k++)
                chain[i,j,k]=false;
        }
        clearer();
        
    }

    void clearer(){


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

    }



    public static void example2tile(int[,,] example, ref int[,,,] Tiles, ref double[] Weights, ref int Tt, int Tx, int Ty, int Tz,bool rotatable = true){
        List<int[,,]> tiles = new List<int[,,]>();
        List<double> w=new List<double>();
        double summ=0;
        int Sx=example.GetLength(0);
        int Sy=example.GetLength(1);
        int Sz=example.GetLength(2);
        int nrot=0;
        if(rotatable)
            nrot=4;
        else
            nrot=1;
        for (int z=0;z<Sz;z++)
        for (int y=0;y<Sy;y++)
        for (int x=0;x<Sx;x++)
        {
            bool is_negative=false;
            int[,,] tile = new int[Tx,Ty,Tz];
            for (int dx=0;dx<Tx;dx++)
            for (int dy=0;dy<Ty;dy++)
            for (int dz=0;dz<Tz;dz++){
                int X=ix(x+dx,Tx);
                int Y=ix(y+dy,Ty);
                int Z=ix(z+dz,Tz);
                if (X<0 || X>=Sx || Y<0 || Y>=Sy || Z<0 || Z>=Sz){
                    tile[dx,dy,dz]=-77;
                    //is_negative=true;
                }
                else
                    tile[dx,dy,dz]=example[X,Y,Z];
            }
            if(!is_negative){
                for(int r=0;r<nrot;r++){
                    int c = check(tile,tiles);
                    summ+=1;
                    if(c<0){
                        tiles.Add(tile);
                        w.Add(1);
                    }
                    else{
                        w[c]+=1;
                    }
                    if (rotatable)
                        tile=rotate(tile,Tx,Ty,Tz);
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

    public static int[,,] rotate( int[,, ] mat,int tx,int ty,int tz)
    {
        int N=tx;
        int[,,] MAT=new int[tx,ty,tz];
        // Consider all
        // squares one by one
        for (int x = 0; x < N / 2; x++) {
            // Consider elements
            // in group of 4 in
            // current square
            for (int y = x; y < N - x - 1; y++) {

                for(int z=0;z<tz;z++){
                // store current cell
                // in temp variable 
                // move values from
                // right to top
                MAT[x, y, z] = mat[y, N - 1 - x, z];
 
                // move values from
                // bottom to right
                MAT[y, N - 1 - x, z]
                    = mat[N - 1 - x, N - 1 - y, z];
 
                // move values from
                // left to bottom
                MAT[N - 1 - x, N - 1 - y, z]
                    = mat[N - 1 - y, x, z];
 
                // assign temp to left
                MAT[N - 1 - y, x, z] = mat[x, y, z];;
                }
            }
        }
        return MAT;
    }

    public static int ix(int idx,int idx_size){
        return idx-idx_size/2+1-idx_size%2;
    }
    


}

