# Domain glossary

## Worker

A person assigned training by an organization. The MVP worker is a newly recruited coal-mine worker.

## Training module

A named safety topic, such as Fire and Explosion Response. A module can have multiple immutable versions.

## Scenario bundle

The versioned procedure, scoring rules, training cues, and scene references needed to conduct one training module offline.

## Training attempt

One worker's execution of one exact scenario-bundle version. The device assigns its identity before the attempt starts.

## Training action

A domain-level worker action such as selecting an extinguisher or entering an exit waypoint. It contains no Unity or ARCore type.

## Attempt event

An ordered record of how the training runtime interpreted a training action.

## Critical failure

An unsafe action that prevents the attempt from passing regardless of its numeric score.

## Training-completion certificate

A verifiable record issued by an authorized organization after the server validates a passing attempt. The prototype does not claim statutory recognition.
