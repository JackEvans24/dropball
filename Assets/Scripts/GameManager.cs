﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    protected GameObject token;
    [SerializeField]
    protected Transform tokenSpawn;
    [SerializeField]
    protected GameObject scoreTag;
    [SerializeField]
    protected GameObject overlay;

    [SerializeField]
    protected float scoreX;
    [SerializeField]
    protected float scoreY;
    [SerializeField]
    protected float scoreYGap;

    protected TurnController turnController;
    protected GameState state;

    protected bool tokenInPlay;

    protected void Start()
    {
        this.turnController = GlobalControl.Instance.turnController;
        this.state = turnController.State;

        if (GlobalControl.Instance.NewGame) {
            state.ResetGame();
            ShufflePlayers();

            turnController.NewGame(this.state);
        }

        CreateTags();

        state.CurrentPlayer.MyTurn = true;
        GlobalControl.Instance.NewGame = false;

        turnController.UpdateUndoRedoButtons();
    }

    private void ShufflePlayers()
    {
        var shuffledPlayers = new List<Player>();
        var players = state.Players;

        while (players.Any())
        {
            var randomPlayerIndex = Random.Range(0, players.Count);

            shuffledPlayers.Add(players[randomPlayerIndex]);
            players.RemoveAt(randomPlayerIndex);
        }

        state.Players = shuffledPlayers;
    }

    protected void CreateTags()
    {
        var newGame = GlobalControl.Instance.NewGame;
        var players = state.Players;

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];

            if (newGame)
            {
                player.Score = 0;
                player.MyTurn = false;
            }

            var tag = CreateTag(i);
            tag.GetComponent<ScoreTag>().player = player;
        }
    }

    protected GameObject CreateTag(int playerIndex)
    {
        var instPos = new Vector3(scoreX, scoreY - (playerIndex * scoreYGap), 0);
        return GameObject.Instantiate(scoreTag, instPos, Quaternion.identity);
    }

    public void SpawnToken(Vector3 spawnPos)
    {
        if (tokenInPlay)
            return;

        var tokenScript = token.GetComponent<Token>();
        tokenScript.tokenColor = state.CurrentPlayer.Colour;
        tokenScript.bounceSound = state.CurrentPlayer.bounceSound?.Sound;

        tokenInPlay = true;

        GameObject.Instantiate(token, spawnPos, Quaternion.identity);
    }

    public virtual void Play()
    {
        overlay.SetActive(false);
        SpawnToken(tokenSpawn.position);
    }

    public virtual void FinishTurn(int pointsAdded)
    {
        tokenInPlay = false;

        turnController.NextTurn(pointsAdded);

        ShowOverlay();

        turnController.UpdateUndoRedoButtons();
    }

    protected void ShowOverlay()
    {
        overlay.SetActive(true);
    }

    public void Incorrect()
    {
        this.FinishTurn(-2);
    }

    public virtual void Undo()
    {
        if (turnController.CanUndo())
            turnController.Undo();
    }

    public virtual void Redo()
    {
        if (turnController.CanRedo())
            turnController.Redo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(new Vector3(scoreX, scoreY, 0), 1f);
    }

    public void StartSuddenDeath()
    {
        GlobalControl.Instance.NewGame = true;
        GlobalControl.LoadScene(Scenes.BoardOne_SuddenDeath);
    }

    public void Quit()
    {
        turnController.EndGame();

        GlobalControl.LoadScene(Scenes.MainMenu);
    }
}
