﻿using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DemoX
{
    public class Program
    {
        private static Action _action = () => { };
        static void Main(string[] args)
        {
            var foobar = new ServiceCollection()
                 .AddSingleton<IFoo, Foo>()
                 .AddSingleton<IBar, Bar>()
                 .AddSingleton(typeof(IFoobar<,>), typeof(Foobar<,>))
                 .BuildInterceptableServiceProvider()
                 .GetRequiredService<IFoobar<IFoo, IBar>>();
            var flag = "";
            _action = () => flag = "Foobar";
            var foo = foobar.Foo;
            Debug.Assert("Foobar" == flag);
        }


        public interface IFoo { }
        public interface IBar { }
        public interface IFoobar<TFoo, TBar>
            where TFoo : IFoo
            where TBar : IBar
        {
            TFoo Foo { get; }
            TBar Bar { get; }
        }
        public class Foo : IFoo { }
        public class Bar : IBar { }

        [Foobar]
        public class Foobar<TFoo, TBar> : IFoobar<TFoo, TBar>
            where TFoo : IFoo
            where TBar : IBar
        {
            public Foobar(TFoo foo, TBar bar)
            {
                this.Foo = foo;
                this.Bar = bar;
            }
            public TFoo Foo { get; }
            public TBar Bar { get; }
        }   
      
        public class FoobarInterceptor
        {
            private InterceptDelegate _next;
            public FoobarInterceptor(InterceptDelegate next)
            {
                _next = next;
            }

            public Task InvokeAsync(InvocationContext context)
            {
                _action();
                return _next(context);
            }
        }

        public class FoobarAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptor>(this.Order);
            }
        }

    }
}
