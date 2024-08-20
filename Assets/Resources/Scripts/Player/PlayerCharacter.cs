using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    public SpriteRenderer viewSprite;


    public override void Update()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        if (JoyStick.instance != null)
        {
            if (JoyStick.instance.isMove)
            {
                JoyStick joyStick = JoyStick.instance;

                isMove = true;
                Vector3 dir = new Vector3(joyStick.Horizontal, joyStick.Vertical, 0);
                dir.Normalize();
                MoveUnit(dir);
                MoveAnimator(dir);
            }
            else
            {
                JoyStick joyStick = JoyStick.instance;

                isMove = false;
                Vector3 dir = new Vector3(joyStick.Horizontal, joyStick.Vertical, 0);
                dir.Normalize();
                MoveAnimator(dir);
            }
        }
    }
    public override void MoveUnit(Vector3 dir)
    {
        if (!isReadyToMove)
        {
            return;
        }
        // �÷��̾��� ���� ��ġ
        Vector3 currentPosition = transform.position;

        float newX = currentPosition.x + dir.x * (float)playStatus.moveSpeed;
        float newY = currentPosition.y + dir.y * (float)playStatus.moveSpeed;

        //����
        if (dir.x < 0)
        {
            viewSprite.flipX = false;
        }
        //������
        else
        {
            viewSprite.flipX = true;
        }

        FollowCamera camera = StageActivity.Instance.followCamera;
        //X ��ǥ�� �ܰ��� ����� �� �̵��� �����մϴ�.
        if (newX <= camera.xMin || newX >= camera.xMax)
        {
            newX = currentPosition.x; // X �̵��� �����ϴ�.
        }

        //Y ��ǥ�� �ܰ��� ����� �� �̵��� �����մϴ�.
        if (newY <= camera.yMin || newY >= camera.yMax)
        {
            newY = currentPosition.y; // Y �̵��� �����ϴ�.
        }

        // ���ο� ��ġ�� �����մϴ�.
        Vector3 nextPosition = new Vector3(newX, newY, currentPosition.z);
        rigid.MovePosition(nextPosition);
        rigid.velocity = Vector3.zero;
    }

    
    public override void MoveAnimator(Vector3 dir)
    {
        if (!isReadyToMove)
            return;

        if (isMove)
        {
            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);
        }

        if (isMove)
        {
            if (dir.x > 0.95f || dir.x < -0.95f)
            {
                anim.SetInteger("Direction", 0);
            }
            else
            {
                //��
                if (dir.y > 0)
                {
                    anim.SetInteger("Direction", 2);
                }
                //�Ʒ�
                else
                {
                    anim.SetInteger("Direction", 1);
                }
            }
        }
    }
}
