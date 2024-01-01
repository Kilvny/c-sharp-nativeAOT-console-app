using AlarmExample;
using NativeAotApi;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Stateless;
internal class Program
{
    static Alarm _alarm;
    private static void Main(string[] args)
    {
        _alarm = new Alarm(10, 10, 10, 10);
        string input = "";

        WriteHeader();

        var startTime = Stopwatch.GetTimestamp();

        var builder = WebApplication.CreateSlimBuilder(args);

        while (input != "q")
        {
            Console.Write("> ");

            input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
                switch (input.Split(" ")[0])
                {
                    case "q":
                        Console.WriteLine("Exiting...");
                        break;
                    case "fire":
                        WriteFire(input);
                        break;
                    case "canfire":
                        WriteCanFire();
                        break;
                    case "state":
                        WriteState();
                        break;
                    case "h":
                    case "help":
                        WriteHelp();
                        break;
                    case "c":
                    case "clear":
                        Console.Clear();
                        WriteHeader();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Type 'h' or 'help' for valid commands.");
                        break;
                }
        }

        static void WriteHelp()
        {
            Console.WriteLine("Valid commands:");
            Console.WriteLine("q               - Exit");
            Console.WriteLine("fire <state>    - Tries to fire the provided commands");
            Console.WriteLine("canfire <state> - Returns a list of fireable commands");
            Console.WriteLine("state           - Returns the current state");
            Console.WriteLine("c / clear       - Clear the window");
            Console.WriteLine("h / help        - Show this again");
        }

        static void WriteHeader()
        {
            Console.WriteLine("Stateless-based alarm test application:");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("");
        }

        static void WriteCanFire()
        {
            foreach (AlarmCommand command in (AlarmCommand[])Enum.GetValues(typeof(AlarmCommand)))
                if (_alarm != null && _alarm.CanFireCommand(command))
                    Console.WriteLine($"{Enum.GetName(typeof(AlarmCommand), command)}");
        }

        static void WriteState()
        {
            if (_alarm != null)
                Console.WriteLine($"The current state is {Enum.GetName(typeof(AlarmState), _alarm.CurrentState())}");
        }

        static void WriteFire(string input)
        {
            if (input.Split(" ").Length == 2)
            {
                try
                {
                    if (Enum.TryParse(input.Split(" ")[1], out AlarmCommand command))
                    {
                        if (_alarm != null)
                            _alarm.ExecuteTransition(command);
                    }
                    else
                    {
                        Console.WriteLine($"{input.Split(" ")[1]} is not a valid AlarmCommand.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"{input.Split(" ")[1]} is not a valid AlarmCommand to the current state.");
                }
            }
            else
            {
                Console.WriteLine("fire requires you to specify the command you want to fire.");
            }
        }

        builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

        var app = builder.Build();

        var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", () => sampleTodos);
        todosApi.MapGet("/{id}", (int id) =>
            sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());


        var lifeTime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        lifeTime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        });

        app.Run();
    }
}

