using UnityEngine;
using System.Collections;

public class LevelMenu : StateMachineBehaviour {

	private Animator topicAnimator;
	private Animator levelAnimator;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		MenuReferences r = animator.gameObject.GetComponent<MenuReferences> ();

		levelAnimator = r.levelAnimator;
		topicAnimator = r.topicAnimator;

		topicAnimator.SetBool ("Show", true);
		levelAnimator.SetBool ("Show", true);
		levelAnimator.SetBool ("Table", false);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		if (animator.GetInteger ("Menu") < 3) {

			levelAnimator.SetBool ("Table", false);
			levelAnimator.SetBool ("Show", false);

		} else {
			
			topicAnimator.SetBool ("Show", false);
			levelAnimator.SetBool ("Table", true);
			levelAnimator.SetBool ("Show", true);

		}
	}
		

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
