# Add this function to ~/.bashrc, ~/.zshrc, or another shell startup file.
# It makes `bp my-bookmark` change the current shell directory.
bp() {
    case "$#:$1" in
        1:list|1:update|1:delete|1:help|1:--help|1:-h)
            command bookpath "$@"
            ;;
        1:*)
            if [[ "$1" != -* ]]; then
                local path
                path="$(command bookpath "$1")" || return
                cd -- "$path" || return
            else
                command bookpath "$@"
            fi
            ;;
        *)
            command bookpath "$@"
            ;;
    esac
}
