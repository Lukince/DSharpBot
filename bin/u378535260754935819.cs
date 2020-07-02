using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static System.Console;

namespace StarFor
{

    public class Repeat_star 
    {
    	static void Main()
    	{
    		Restart:
    		int total = 0;
    		int count = 0;
    		int WrongCount = 0;
    		Input:
    		try
    		{
    			Write("반복 횟수를 입력하세요 ( Stop to put zero : ");
    			count = int.Parse(ReadLine());
    			if (count < 0)
    			{
    				WrongCount++;
    				if (WrongCount == 3)
    				{
    					WriteLine("You put wrong 3 times");
    					goto End;
    				}
    				WriteLine("0보다 작거나 같은 수는 입력할 수 없습니다.");
    				goto Input;
    			}
    			else if (count == 0)
    			{
    				goto End;
    			}
    		}
    		catch(Exception e)
    		{
    			WriteLine($"Unknown Error. If you want to end, put zero.\nRestarting Program...");
    			goto Restart;
    		}
    		
    		for (int i = 0; i < count; i++)
    		{
    			for (int j = 0; j <= i; j++)
    			{
    				total++;
    				Write("*");
    			}
    			WriteLine();
    		}
    		WriteLine($"Total : {total}");
    		
    		End:
    		WriteLine("Program ended");
    		
    	}
    }
}