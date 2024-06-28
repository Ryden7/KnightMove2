using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace KnightMove2;

public class Function
{
    // Directions the knight can move
    private static readonly int[] dx = { 2, 2, 1, 1, -1, -1, -2, -2 };
    private static readonly int[] dy = { 1, -1, 2, -2, 2, -2, 1, -1 };
    private string accessKeyId = Environment.GetEnvironmentVariable("AccessKeyId");
    private string secretAccessKey = Environment.GetEnvironmentVariable("SecretAccessKey");

    /// <summary>
    /// A simple function that takes KnightPathResponse, calculates the shortest path, and stores it in S3 storage.
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<bool> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        KnightPathResponse knightPathResponse = null;
        try
        {
            // Check if the request body is null or empty
            if (string.IsNullOrEmpty(request.Body))
            {
                context.Logger.Log("Request body is null or empty.");
                return false;
            }

            // Deserialize the request body into KnightPathResponse
            knightPathResponse = JsonConvert.DeserializeObject<KnightPathResponse>(request.Body);

            if (knightPathResponse == null)
            {
                context.Logger.Log("Deserialization returned null.");
                return false;
            }

            // Process the deserialized object (knightPathResponse)
            context.Logger.Log("Processed KnightPathResponse: " + knightPathResponse);

        }
        catch (Exception ex)
        {
            context.Logger.Log($"Error: {ex.Message}");
            return false;
        }

        var startCoordinate = ChessToCoords(knightPathResponse.Starting);
        var endCoordinate = ChessToCoords(knightPathResponse.Ending);
        S3Helper s3Helper = new S3Helper(accessKeyId, secretAccessKey, "us-east-1");
        if (startCoordinate.XAxisPoint == endCoordinate.XAxisPoint && startCoordinate.YAxisPoint == endCoordinate.YAxisPoint)
        {
            Console.WriteLine("Start and end positions are the same. Moves: 0");
            return true;
        }

        Queue<CoordinateToCalculate> queue = new Queue<CoordinateToCalculate>();
        bool[,] visited = new bool[8, 8];
        CoordinateToCalculate coordinateToCalculate = new CoordinateToCalculate(startCoordinate.XAxisPoint, startCoordinate.YAxisPoint, 0, new List<string> { knightPathResponse.Starting });
        queue.Enqueue(coordinateToCalculate);
        visited[startCoordinate.XAxisPoint, startCoordinate.YAxisPoint] = true;

        while (queue.Count > 0)
        {
            var coordinateToProcess = queue.Dequeue();

            for (int i = 0; i < dx.Length; i++)
            {
                int newX = coordinateToProcess.x + dx[i];
                int newY = coordinateToProcess.y + dy[i];

                if (IsValid(newX, newY) && !visited[newX, newY])
                {
                    visited[newX, newY] = true;
                    var newPath = new List<string>(coordinateToProcess.path) { CoordsToChess(newX, newY) };

                    if (newX == endCoordinate.XAxisPoint && newY == endCoordinate.YAxisPoint)
                    {
                        var shortestPath = string.Join(":", newPath);
                        knightPathResponse.ShortestPath = shortestPath;
                        knightPathResponse.NumberOfMoves = coordinateToProcess.dist + 1;
                        var kPRSerialized = JsonConvert.SerializeObject(knightPathResponse);
                        await s3Helper.WriteObjectToS3Async("kprbucket", knightPathResponse.OperationId, kPRSerialized);
                        return true;
                    }

                    var adjacentCoord = new CoordinateToCalculate(newX, newY, coordinateToProcess.dist + 1, newPath);
                    queue.Enqueue(adjacentCoord);
                }
            }
        }

        return false;
    }

    // Method to convert chess notation to coordinates
    private static BoardCoordinate ChessToCoords(string position)
    {
        int x = position[0] - 'A';
        int y = position[1] - '1';
        return new BoardCoordinate(x, y);
    }

    // Method to convert coordinates to chess notation
    private static string CoordsToChess(int x, int y)
    {
        char col = (char)(x + 'A');
        char row = (char)(y + '1');
        return $"{col}{row}";
    }

    // Method to check if a position is within the board
    private static bool IsValid(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}
