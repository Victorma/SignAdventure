using UnityEngine;
using System.Collections;

public class Start : StateMachineBehaviour {

	private Animator homeScreenAnimator;
	private Animator startButtonAnimator;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		homeScreenAnimator = animator.gameObject.GetComponent<MenuReferences> ().homeAnimator;
		startButtonAnimator = animator.gameObject.GetComponent<MenuReferences> ().startButton;
		homeScreenAnimator.SetBool ("Show", true);
		startButtonAnimator.SetBool ("Fly", false);

	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {


		homeScreenAnimator.SetBool ("Show", false);
		startButtonAnimator.SetBool ("Fly", true);

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
