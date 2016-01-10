using System;

namespace DisposableDemo
{
    using static Console;

    class Program : DisposableBase
    {
        static void Main(string[] args)
        {
            using (var instance = new Program())
                instance.Dispose();

            new Program();
            GC.Collect();

            ReadKey();
        }

        protected override void Dispose(bool disposing)
            => WriteLine(disposing);
    }
}
