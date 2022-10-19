using Newtonsoft.Json;
using System.Collections.Generic;

public class GameStateData {
	[JsonProperty("gameId")]
	public string GameId { get; set; }

	[JsonProperty("gameStatus")]
	public string GameStatus { get; set; }

	[JsonProperty("gameStartTimestamp")]
	public long GameStartTimestamp { get; set; } //in milliseconds

	[JsonProperty("locations")]
	public List<Seat> Locations { get; set; }

	public override string ToString() {
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}

public class Seat {
	[JsonProperty("location")]
	public int Location { get; set; }

	[JsonProperty("score")]
	public int Score { get; set; }

	[JsonProperty("playerName")]
	public string PlayerName { get; set; }

	[JsonProperty("playerId")]
	public string PlayerId { get; set; }

	public override string ToString() {
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}

[System.Serializable]
public class GameStateMessage {
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; } //in milliseconds

	[JsonProperty("data")]
	public GameStateData Data { get; set; }
}