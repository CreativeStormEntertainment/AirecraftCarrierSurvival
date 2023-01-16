
public interface IInteractive
{
    void OnHoverEnter();
    void OnHoverExit();
    void OnHoverStay();
    float GetHoverStayTime();
    void OnClickStart();
    void OnClickHold();
    void OnClickEnd(bool success);
    float GetClickHoldTime();
}
