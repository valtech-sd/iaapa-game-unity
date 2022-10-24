using Newtonsoft.Json;
using System.Collections.Generic;

public enum MessageType {
	GameState,
	Leaderboard
}

public enum GameState {
	Idle,
	Load,
	Run,
	End
}


public class GameFlags {
	[JsonProperty("leaderboardEnabled")]
	public bool LeaderboardEnabled { get; set; }
}
public class GameStateData {
	[JsonProperty("gameId")]
	public string GameId { get; set; }

	[JsonProperty("gameStatus")]
	public string GameStatus { get; set; }

	[JsonProperty("gameStartTimestamp")]
	public long GameStartTimestamp { get; set; } //in milliseconds

	[JsonProperty("flags")]
	public GameFlags Flags { get; set; }

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



[System.Serializable]
public class LeaderboardData {
	[JsonProperty("leaderboardType")]
	public string LeaderboardType { get; set; }

	[JsonProperty("leaderboard")]
	public List<LeaderboardEntry> Leaderboard { get; set; }
}

[System.Serializable]
public class LeaderboardEntry {
	[JsonProperty("rank")]
	public int Rank { get; set; }

	[JsonProperty("score")]
	public int Score { get; set; }

	[JsonProperty("playerName")]
	public string PlayerName { get; set; }

	[JsonProperty("playerId")]
	public string PlayerId { get; set; }
}

[System.Serializable]
public class LeaderboardMessage
{
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; } //in milliseconds

	[JsonProperty("data")]
	public LeaderboardData Data { get; set; }
}
