class Program{
    static void Test(int a){
        foreach(var i in range(0,5)){
            print(a - i);
        }
    }

    static void Main(){
        Test(12);
        print("HelloWorld");
        print(7);
    }
}