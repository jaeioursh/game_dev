using System;


static class Program
{
    static void Main()
    {
        int[,,] map = new int[,,] { { {0,0,1,0,0 }
                                    , {0,0,1,0,0 } 
                                    , {1,1,1,1,1 } 
                                    , {0,0,1,0,0 } 
                                    , {0,0,1,0,0 } 
                                   },};

        int[,,] mapr = new int[,,] { { {1,1,1,1,1,1,1 }
                                    , {1,1,1,1,1,1,1 }
                                    , {1,1,1,1,1,1,1 } 
                                    , {1,1,1,1,1,1,1 }
                                    , {1,1,1,1,1,1,1 } 
                                    , {1,1,1,1,1,1,1 } 
                                    , {1,1,1,1,1,1,1 } 
                                   },};

        int[,,] mapq = new int[,,] { { {1,0,1,0,1 }
                                    , {0,1,0,1,0 } 
                                    , {1,0,1,0,1 } 
                                    , {0,1,0,1,0 } 
                                    , {1,0,1,0,1 } 
                                   },};
        int Z=map.GetLength(0);
        int X=map.GetLength(1);
        int Y=map.GetLength(2);

        int[,,] map2 = new int[X,Y,Z];

        for (int dx=0; dx<X; dx++)
        for (int dy=0; dy<Y; dy++)
        for (int dz=0; dz<Z; dz++)
        map2[dx,dy,dz] = map[dz,dx,dy];

        //int[,,] map5 = img.template("samples/Rooms.png");
        int[,,] map5 = img.template("samples/SimpleWall.png");
        markov wave=new markov(map5,3,3,3,81,81,1);
        

        for (int t=0;t<wave.tt;t++){
            Console.WriteLine("tile: "+t.ToString());
        for (int z=1;z<2;z++){
        for (int y=0;y<wave.ty;y++){
        for (int x=0;x<wave.tx;x++){
        
            if(wave.tiles[x,y,z,t]>=0)
                Console.Write(" ");

            Console.Write(wave.tiles[x,y,z,t]);
        }
        Console.WriteLine(" ");
        }
         Console.WriteLine(" ");
        }
          Console.WriteLine(" ");
        }

        
        int[,,] map3 = null;
        int tryy=0;
        map3 = wave.run(5000,ref tryy);
        
        while (map3 == null){
            int depth=0;
            map3 = wave.run(5000,ref depth);
            tryy++;
            Console.WriteLine(depth);
            
        }
        
        for (int dy=0; dy<map3.GetLength(1); dy++){
            for (int dx=0; dx<map3.GetLength(0); dx++){  
                Console.Write(map3[dx,dy,0]);
                Console.Write(" ");
            }
            Console.WriteLine("");
        }

        img.save("test.png",map3);
        
        return;
    }
}
