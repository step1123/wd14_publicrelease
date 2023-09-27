#!/bin/bash

echo "Moony's upstream merge workflow tool."
echo "This tool can be stopped at any time, i.e. to finish a merge or resolve conflicts. Simply rerun the tool after having resolved the merge with normal git CLI."
echo "Pay attention to any output from git! DO NOT RUN THIS ON A WORKING TREE WITH UNCOMMITTED FILES OF ANY KIND."

read -p "Enter the branch you're syncing toward (typically upstream/master or similar): " target
refs=$(git log --reverse --format=format:%H HEAD.."$target")

git config user.email "92106367+melanoTurbo@users.noreply.github.com"
git config user.name "melano"

options=("Cherry-pick" "Merge" "Skip")
g
for unmerged in $refs; do
    summary=$(git show --format=format:%s "$unmerged")

    if [ "$summary" == "automatic changelog update" ]; then
        echo "Deliberately skipping changelog bot commit $unmerged."
        echo "== GIT (CONFLICTS ARE OKAY) =="
        git merge --no-ff --no-commit --no-verify "$unmerged"
        # DELIBERATELY IGNORE merge conflict markers. We're just going to undo the commit!
        git add .
        git commit -m "squash! Merge tool skipping '$unmerged'"
        newhead=$(git log -n 1 --format=format:%H)
        git reset HEAD~ --hard
        git reset "$newhead" --soft
        git commit --amend --no-edit
        echo "== DONE =="
        continue
    fi

    git show --format=full --summary "$unmerged"

    PS3="Commit action? "
    select option in "${options[@]}"; do
        case $REPLY in
            1)
                echo "== GIT =="
                git cherry-pick "$unmerged"
                echo "== DONE =="
                break
                ;;
            2)
                echo "== GIT =="
                git merge --no-ff -m "squash! Merge tool integrating '$unmerged'" "$unmerged"
                echo "== DONE =="
                break
                ;;
            3)
                echo "Skipping $unmerged"
                echo "== GIT (CONFLICTS ARE OKAY) =="
                git merge --no-ff --no-commit --no-verify "$unmerged"
                # DELIBERATELY IGNORE merge conflict markers. We're just going to undo the commit!
                git add .
                git commit -m "squash! Merge tool skipping '$unmerged'"
                newhead=$(git log -n 1 --format=format:%H)
                git reset HEAD~ --hard
                git reset "$newhead" --soft
                git commit --amend --no-edit
                echo "== DONE =="
                break
                ;;
            *)
                echo "Invalid option, please select a valid option."
                ;;
        esac
    done
done
