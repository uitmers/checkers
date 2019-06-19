using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public GameObject highlightsContainer;

    public CanvasGroup alertCanvas;
    private float lastAlert;

    public Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    public Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    /// <summary>
    /// Ustawienia początkowe programu
    /// </summary>
    private void Start()
    {
        foreach(Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }
        Alert("Biali");
        isWhite = true;
        isWhiteTurn = true;
        forcedPieces = new List<Piece>();
        GenerateBoard();
        
    }
    /// <summary>
    /// Zmiana tury i położenie - ciągła aktualizacja
    /// </summary>
    private void Update()
    {
        foreach (Transform t in highlightsContainer.transform)
        {
            t.Rotate(Vector3.up * 90 * Time.deltaTime);
        }
        UpdateMouseOver();

        if((isWhite)?isWhiteTurn:!isWhiteTurn)
        //If it is my turn
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);
            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }
    }
    /// <summary>
    /// Odczytywanie położenia kursora 
    /// </summary>
    private void UpdateMouseOver()
    {
   

        if(!Camera.main)
        {
            Debug.Log("Nie udało się znaleźć głównej kamery");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    /// <summary>
    /// Podnoszenie figur
    /// </summary>
    /// <param name="p"></param>
    private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Nie udało się znaleźć głównej kamery");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }
    /// <summary>
    /// Wybieranie figur i wymuszony ruch
    /// </summary>
    /// <param name="x" - ID planszy w poziomie></param>
    /// <param name="y" - ID planszy w pionie ></param>
    private void SelectPiece (int x,int y)
    {
        //Ruchy za planszę
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;

        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                //Szukanie figur w liście które figury muszą wykonać ruch
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;

                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
    }
    /// <summary>
    /// Poruszanie się figur, ruchy dozwolone, zbijanie figur
    /// </summary>
    /// <param name="x1" - Położenie początkowe figury w poziomie ></param>
    /// <param name="y1" - Położenie początkowe figury w pionie ></param>
    /// <param name="x2" - Położenie końcowe figury w poziomie ></param>
    /// <param name="y2" - Położenie końcowe figury w pionie ></param>
    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //Za planszą
        if(x2 <0 || x2 >= 8 || y2<0 || y2 >= 8)
        {
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1);

            startDrag = Vector2.zero;
            selectedPiece = null;
            Highlight();
            return;
        }
        if (selectedPiece != null)
        {
            //Jeżeli figura się nie ruszyła
            if(endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                Highlight();
                return;
            }
            //Sprawdzanie czy ruch jest dozwolony
            if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                //Czy figura została zbita podczas ruchu
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p !=null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        DestroyImmediate(p.gameObject);
                        hasKilled = true;
                    }
                }

                //Czy mieliśmy zbić figurę
                if(forcedPieces.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    Highlight();
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                Highlight();
                return;
            }
        }
    }
    /// <summary>
    /// Co ma się stać na koniec tury
    /// </summary>
    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        //Promocje na króla
        if(selectedPiece !=null)
        {
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
            }
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
        if (isWhite)
        {
            Alert("Biali");
        }
        else
        {
            Alert("Czarni");
        }
        ScanForPossibleMove();
    }
    /// <summary>
    /// Sprawdzenie czy któryś z graczy wygrał
    /// </summary>
    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isWhite" - Czy biali wygrali ></param>
    private void Victory (bool isWhite)
    {
        if (isWhite)
            Debug.Log("Biali wygrali");
        else
            Debug.Log("Czarni wygrali");
    }
    /// <summary>
    /// Figury które muszą wykonać ruch
    /// </summary>
    /// <param name="p" - ID figury z planszy ></param>
    /// <param name="x" - Położenie figury w poziomie ></param>
    /// <param name="y" - Położenie figury w pionie ></param>
    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].IsForceToMove(pieces, x, y))
            forcedPieces.Add(pieces[x, y]);

        Highlight();
        return forcedPieces;
    }
    /// <summary>
    /// Figury które muszą wykonać ruch
    /// </summary>
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();

        //Sprawdzenie dla wszystkich figur mozliwych ruchów
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    if (pieces[i, j].IsForceToMove(pieces, i, j))
                        forcedPieces.Add(pieces[i, j]);

        Highlight();
        return forcedPieces;
    }
    /// <summary>
    /// Podświetlanie figur które musza wykonać ruch
    /// </summary>
    private void Highlight()
    {
        foreach(Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (forcedPieces.Count > 0)
            highlightsContainer.transform.GetChild(0).transform.position = forcedPieces[0].transform.position + Vector3.up * 0.01f;
        if (forcedPieces.Count > 1)
            highlightsContainer.transform.GetChild(1).transform.position = forcedPieces[1].transform.position + Vector3.up * 0.01f;
    }
    /// <summary>
    /// Tworzenie figur na planszy
    /// </summary>
    private void GenerateBoard()
    {
// Tworzenie białych figur
for (int y = 0; y<3;y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x= 0; x<8; x+=2)
            {
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }
        // Tworzenie czarnych figur
        for (int y = 7; y > 4; y--  )
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }
    }
    /// <summary>
    /// Tworzenie figur na planszy
    /// </summary>
    /// <param name="x" - Miejsce gdzie ma się znajdować figura w poziomie ></param>
    /// <param name="y" - Miejsce gdzie ma się znajdować figura w pionie ></param>
    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p" - ID figury z planszy ></param>
    /// <param name="x" - położenie figury w poziomie ></param>
    /// <param name="y" - położenie figury w pionie ></param>
    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    /// <summary>
    /// Powiadomienia użytkownika o turze
    /// </summary>
    /// <param name="text" - Tekst który ma być wyświetlany></param>
    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
    }
}
