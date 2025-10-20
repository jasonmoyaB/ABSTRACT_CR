# Branch cleanup guide

This repository previously used several working branches prefixed with `codex/` to deliver the meal ordering functionality.  All of those branches have already been merged into the long-lived branch (`work`) so it is safe to remove the temporary branches without losing any code.

## 1. Confirm the code is merged

```bash
git checkout work
git log --oneline --graph --decorate
```

Verify that each `codex/*` branch tip appears in the history as a merge commit (for example `codex/add-dbsetpedido-and-dbsetpedidodetalle-class`, `codex/add-pedido-and-pedidodetalle-class`, `codex/define-ipedioservice-and-dtos`, `codex/generate-pedidoscontroller-with-views`).

## 2. Create/update the main branch

If `master` should become the default branch, fast-forward it to the latest `work` commit:

```bash
git checkout master # or create it if it does not exist yet
git merge --ff-only work
```

If `master` does not exist locally yet, create it from `work`:

```bash
git checkout work
git branch master
```

Push the updated default branch to the remote when ready:

```bash
git push origin master
```

## 3. Delete the temporary branches

After confirming that the remote default branch contains the desired code, delete the `codex/*` branches both locally and remotely:

```bash
# delete locally
git branch -d codex/add-dbsetpedido-and-dbsetpedidodetalle-class
git branch -d codex/add-pedido-and-pedidodetalle-class
git branch -d codex/define-ipedioservice-and-dtos
git branch -d codex/generate-pedidoscontroller-with-views

# delete remotely
git push origin --delete codex/add-dbsetpedido-and-dbsetpedidodetalle-class
git push origin --delete codex/add-pedido-and-pedidodetalle-class
git push origin --delete codex/define-ipedioservice-and-dtos
git push origin --delete codex/generate-pedidoscontroller-with-views
```

> :warning: Only delete the remote branches once you have confirmed their commits are present on the branch that will remain active.

## 4. Update pull request templates or automations (optional)

If any CI/CD or PR templates referenced the `codex/*` branches, update them to point to the surviving branch (`master` or `work`).

Following these steps lets you remove the temporary `codex/…` branches while preserving the features that they delivered.
