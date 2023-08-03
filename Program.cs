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
        int Z=map.GetLength(0);
        int X=map.GetLength(1);
        int Y=map.GetLength(2);

        int[,,] map2 = new int[X,Y,Z];
        for (int dx=0; dx<X; dx++)
        for (int dy=0; dy<Y; dy++)
        for (int dz=0; dz<Z; dz++)
        map2[dx,dy,dz] = map[dz,dx,dy];

        
        wfc wave=new wfc(map2,5,5,1,7,7,1);
        
        for (int t=0;t<wave.tt;t++){
            Console.WriteLine("tile: "+t.ToString());
        for (int z=0;z<wave.tz;z++){
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
        wave.run();
        int[,,] map3 = wave.result();
        for (int dx=0; dx<map3.GetLength(0); dx++){
            for (int dy=0; dy<map3.GetLength(1); dy++){
                Console.Write(map3[dx,dy,0]);
            }
            Console.WriteLine("");
        }

        return;
    }
}
